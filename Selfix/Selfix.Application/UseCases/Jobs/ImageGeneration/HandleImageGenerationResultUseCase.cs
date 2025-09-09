using LanguageExt;
using LanguageExt.Common;
using Selfix.Application.Dtos.Images;
using Selfix.Application.Dtos.TelegramProfiles;
using Selfix.Application.ServicesAbstractions.Database;
using Selfix.Application.ServicesAbstractions.Database.Repositories;
using Selfix.Application.ServicesAbstractions.Database.Repositories.Cached;
using Selfix.Application.ServicesAbstractions.Environment;
using Selfix.Application.ServicesAbstractions.ObjectStorage;
using Selfix.Application.ServicesAbstractions.Telegram;
using Selfix.Domain.Entities.Images;
using Selfix.Domain.Entities.Images.Specifications;
using Selfix.Domain.Entities.Jobs;
using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Images;
using Selfix.Domain.ValueObjects.Jobs;
using Selfix.Domain.ValueObjects.Jobs.ImageGeneration;
using Selfix.Domain.ValueObjects.ObjectStorage;
using Selfix.Shared;
using Selfix.Shared.Extensions;
using Serilog;

namespace Selfix.Application.UseCases.Jobs.ImageGeneration;

internal sealed class HandleImageGenerationResultUseCase : IHandleImageGenerationResultUseCase
{
    private readonly IEnvironmentService _environmentService;
    private readonly IImagesRepository _imagesRepository;
    private readonly IJobsRepository _jobsRepository;
    private readonly IObjectStorageService _objectStorageService;
    private readonly ITelegramProfilesRepository _telegramProfilesRepository;
    private readonly ITelegramService _telegramService;
    private readonly ITransactionService _transactionService;
    private readonly IUsersRepository _usersRepository;
    private readonly ILogger _logger;

    public HandleImageGenerationResultUseCase(IJobsRepository jobsRepository, IEnvironmentService environmentService,
        ITransactionService transactionService, ITelegramService telegramService,
        CachedTelegramProfilesRepository telegramProfilesRepository, IObjectStorageService objectStorageService,
        IImagesRepository imagesRepository, CachedUsersRepository usersRepository, ILogger logger)
    {
        _jobsRepository = jobsRepository;
        _environmentService = environmentService;
        _transactionService = transactionService;
        _telegramService = telegramService;
        _telegramProfilesRepository = telegramProfilesRepository;
        _objectStorageService = objectStorageService;
        _imagesRepository = imagesRepository;
        _usersRepository = usersRepository;
        _logger = logger;
    }

    public IO<HandleImageGenerationResultResponse> Execute(HandleImageGenerationResultRequest request,
        CancellationToken cancellationToken)
    {
        var jobId = Id<Job, Ulid>.FromSafe(request.JobId);
        return
            from job in _jobsRepository.GetById(jobId, cancellationToken)
                .ToIOFailIfNone(Error.New($"Job with id: {request.JobId} not found"))
            from _1 in request.GenerationResultData switch
            {
                SucceedImageGenerationData data => HandleImageGenerationSuccess(job, data, cancellationToken),
                FailedImageGenerationData data => HandleImageGenerationFail(job, data, cancellationToken),
                _ => Error.New(
                    $"There is no handler for data type: {request.GenerationResultData.GetType()}")
            }
            select new HandleImageGenerationResultResponse();
    }

    private IO<Unit> HandleImageGenerationSuccess(Job job, SucceedImageGenerationData data,
        CancellationToken cancellationToken)
    {
        if (job.Status.IsCompleted) return IO<Unit>.Pure(Unit.Default);
        return
            from currentTime in _environmentService.GetCurrentTime(cancellationToken)
            from jobData in job.SetOutput(new ImageGenerationJobOutput()).ToIO()
            from _1 in job.ChangeStatus(new JobStatus(JobStatusEnum.Succeeded, currentTime)).ToIO()
            from imagesPaths in data.Paths.AsIterable().Traverse(OSFilePath.From).As().ToIO()
            from images in CreateImages(imagesPaths, job.UserId, currentTime).ToIO()
            from _2 in _transactionService.Run(
                from _1 in _jobsRepository.Save(job, cancellationToken)
                from _2 in SaveImages(images, cancellationToken)
                from _3 in SendImagesToTelegramUser(job, images, cancellationToken)
                //.IfFail(_ => IO<Unit>.Pure(Unit.Default)) - will ignore it when there are multiple profiles
                select Unit.Default, cancellationToken)
            select Unit.Default;
    }

    private static Fin<Iterable<Image>> CreateImages(Iterable<OSFilePath> paths, Id<User, Ulid> userId,
        DateTimeOffset currentTime)
    {
        return paths.Traverse(p => Image.New(new NewImageSpecification(p, currentTime, userId))).As();
    }

    private IO<Unit> SaveImages(Iterable<Image> images, CancellationToken cancellationToken)
    {
        return images.TraverseM(img => _imagesRepository.Save(img, cancellationToken)).IgnoreF().As();
    }

    private IO<Unit> SendImagesToTelegramUser(Job job, Iterable<Image> images, CancellationToken cancellationToken)
    {
        return
            from telegramProfile in _telegramProfilesRepository.GetByUserId(job.UserId, cancellationToken)
                .ToIOFailIfNone(Error.New($"There is no telegram profile for user with id: {job.UserId}"))
            from bucketName in _objectStorageService.GetBucketName(BucketContentTypeEnum.Images)
            from signedUris in images.TraverseM(img =>
                _objectStorageService.GetSignedUriForReading(OSFileLocation.Create(bucketName, img.Path), cancellationToken))
            let imagesWithSignedUris = images.Map(img => img.ToDto()).Zip(signedUris)
            from _1 in _telegramService.ShowPhotoWidgets(telegramProfile.ToDto(), imagesWithSignedUris, cancellationToken)
            select Unit.Default;
    }

    private IO<Unit> HandleImageGenerationFail(Job job, FailedImageGenerationData data,
        CancellationToken cancellationToken)
    {
        return
            from currentTime in _environmentService.GetCurrentTime(cancellationToken)
            from jobData in job.Data switch
            {
                ImageGenerationJobData jobData => from notes in Notes.From(data.Error).ToIO()
                    from _1 in job.AddNotes(notes).ToIO()
                    from _2 in job.ChangeStatus(new JobStatus(JobStatusEnum.Failed, currentTime)).ToIO()
                    from user in _usersRepository.GetById(job.UserId, cancellationToken)
                        .ToIOFailIfNone(Error.New($"User with id: {job.UserId} not found"))
                    let _3 = user.AddImageGenerations(jobData.Input.Quantity)
                    from _4 in _transactionService.Run(
                        from _1 in _jobsRepository.Save(job, cancellationToken)
                        from _2 in _usersRepository.Save(user, cancellationToken)
                        from _3 in NotifyTelegramUserThatImagesGenerationFailed(job, jobData, cancellationToken)
                        //.IfFail(_ => IO<Unit>.Pure(Unit.Default)) - will ignore it when there are multiple profiles
                        select Unit.Default, cancellationToken)
                    select Unit.Default,
                _ => Error.New($"There is no handler for data type: {job.Data.GetType()}")
            }
            select Unit.Default;
    }

    private IO<Unit> NotifyTelegramUserThatImagesGenerationFailed(Job job, ImageGenerationJobData _,
        CancellationToken cancellationToken)
    {
        return
            from telegramProfile in _telegramProfilesRepository.GetByUserId(job.UserId, cancellationToken)
                .ToIOFailIfNone(Error.New($"There is no telegram profile for user with id: {job.UserId}"))
            from _1 in _telegramService.ShowImageGenerationFailedWidget(telegramProfile.ToDto(), cancellationToken)
            select Unit.Default;
    }
}