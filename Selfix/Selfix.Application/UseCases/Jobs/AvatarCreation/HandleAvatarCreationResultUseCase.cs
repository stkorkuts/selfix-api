using LanguageExt;
using LanguageExt.Common;
using Selfix.Application.Dtos.TelegramProfiles;
using Selfix.Application.ServicesAbstractions.Database;
using Selfix.Application.ServicesAbstractions.Database.Repositories;
using Selfix.Application.ServicesAbstractions.Database.Repositories.Cached;
using Selfix.Application.ServicesAbstractions.Environment;
using Selfix.Application.ServicesAbstractions.ObjectStorage;
using Selfix.Application.ServicesAbstractions.Telegram;
using Selfix.Domain.Entities.Avatars;
using Selfix.Domain.Entities.Avatars.Specifications;
using Selfix.Domain.Entities.Jobs;
using Selfix.Domain.ValueObjects.Avatars;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Jobs;
using Selfix.Domain.ValueObjects.Jobs.AvatarCreation;
using Selfix.Domain.ValueObjects.ObjectStorage;
using Selfix.Shared;
using Selfix.Shared.Extensions;

namespace Selfix.Application.UseCases.Jobs.AvatarCreation;

internal sealed class HandleAvatarCreationResultUseCase : IHandleAvatarCreationResultUseCase
{
    private readonly IAvatarsRepository _avatarsRepository;
    private readonly IEnvironmentService _environmentService;
    private readonly IJobsRepository _jobsRepository;
    private readonly IObjectStorageService _objectStorageService;
    private readonly ITelegramProfilesRepository _telegramProfilesRepository;
    private readonly ITelegramService _telegramService;
    private readonly ITransactionService _transactionService;
    private readonly IUsersRepository _usersRepository;

    public HandleAvatarCreationResultUseCase(IJobsRepository jobsRepository, IAvatarsRepository avatarsRepository,
        IObjectStorageService objectStorageService, IEnvironmentService environmentService,
        ITelegramService telegramService,
        CachedTelegramProfilesRepository telegramProfilesRepository, ITransactionService transactionService,
        CachedUsersRepository usersRepository)
    {
        _jobsRepository = jobsRepository;
        _avatarsRepository = avatarsRepository;
        _objectStorageService = objectStorageService;
        _environmentService = environmentService;
        _telegramService = telegramService;
        _telegramProfilesRepository = telegramProfilesRepository;
        _transactionService = transactionService;
        _usersRepository = usersRepository;
    }

    public IO<HandleAvatarCreationResultResponse> Execute(HandleAvatarCreationResultRequest creationResultRequest,
        CancellationToken cancellationToken)
    {
        var jobId = Id<Job, Ulid>.FromSafe(creationResultRequest.JobId);
        return
            from job in _jobsRepository.GetById(jobId, cancellationToken)
                .ToIOFailIfNone(Error.New($"Job with id: {creationResultRequest.JobId} not found"))
            from _1 in creationResultRequest.CreationResultData switch
            {
                SucceedAvatarCreationData data => HandleSuccessfullyCreatedAvatar(job, data, cancellationToken),
                FailedAvatarCreationData data => HandleAvatarCreationFailed(job, data, cancellationToken),
                _ => Error.New(
                    $"There is no handler for data type: {creationResultRequest.CreationResultData.GetType()}")
            }
            select new HandleAvatarCreationResultResponse();
    }

    private IO<Unit> HandleSuccessfullyCreatedAvatar(Job job, SucceedAvatarCreationData data,
        CancellationToken cancellationToken)
    {
        return
            from user in _usersRepository.GetById(job.UserId, cancellationToken)
                .ToIOFailIfNone(Error.New($"User with id: {job.UserId} not found"))
            from currentTime in _environmentService.GetCurrentTime(cancellationToken)
            // TODO: Add job output values
            from jobData in job.SetOutput(new AvatarCreationJobOutput()).ToIO()
            from _1 in job.ChangeStatus(new JobStatus(JobStatusEnum.Succeeded, currentTime)).ToIO()
            from avatar in CreateAvatar(job, data, jobData.Input.Name, currentTime).ToIO()
            from _2 in user.SetActiveAvatarId(avatar).ToIO()
            from _3 in _transactionService.Run(
                from _1 in _avatarsRepository.Save(avatar, cancellationToken)
                from _2 in _jobsRepository.Save(job, cancellationToken)
                from _3 in _usersRepository.Save(user, cancellationToken)
                from _4 in NotifyTelegramUserThatAvatarCreated(job, avatar, cancellationToken)
                //.IfFail(_ => IO<Unit>.Pure(Unit.Default)) - will ignore it when there are multiple profiles
                from _5 in DeleteImagesFromObjectStorage(jobData.Input.ImagePaths, cancellationToken)
                select Unit.Default, cancellationToken)
            select Unit.Default;
    }

    private static Fin<Avatar> CreateAvatar(Job job, SucceedAvatarCreationData creationData, AvatarName name,
        DateTimeOffset currentTime)
    {
        return
            from avatarDescription in AvatarDescription.From(creationData.Description)
            from loraPath in OSFilePath.From(creationData.LoraPath)
            from avatar in Avatar.New(new NewAvatarSpecification(job.UserId, name, avatarDescription, loraPath,
                currentTime))
            select avatar;
    }

    private IO<Unit> DeleteImagesFromObjectStorage(Iterable<OSFilePath> filePaths, CancellationToken cancellationToken)
    {
        return
            from bucketName in _objectStorageService.GetBucketName(BucketContentTypeEnum.Temporary)
            let fileLocations = filePaths.Map(fp => OSFileLocation.Create(bucketName, fp))
            from _1 in fileLocations.TraverseM(f => _objectStorageService.DeleteFileIfExist(f, cancellationToken))
            select Unit.Default;
    }

    private IO<Unit> NotifyTelegramUserThatAvatarCreated(Job job, Avatar avatar, CancellationToken cancellationToken)
    {
        var message = $"Аватар {avatar.Name} создан и активирован!\n" +
                      $"Теперь вы можете использовать его для генерации своих изображений!\n" +
                      $"Для генерации изображений перейдите в меню и выберите образ из списка образов или выберите режим творца для создания своего собственного образа";
        return
            from telegramProfile in _telegramProfilesRepository.GetByUserId(job.UserId, cancellationToken)
                .ToIOFailIfNone(Error.New($"There is no telegram profile for user with id: {job.UserId}"))
            from _1 in _telegramService.ShowSimpleNotificationWidget(telegramProfile.ToDto(), message,
                cancellationToken)
            select Unit.Default;
    }

    private IO<Unit> HandleAvatarCreationFailed(Job job, FailedAvatarCreationData data,
        CancellationToken cancellationToken)
    {
        return
            from currentTime in _environmentService.GetCurrentTime(cancellationToken)
            // TODO: Add job output values
            from jobData in job.SetOutput(new AvatarCreationJobOutput()).ToIO()
            from notes in Notes.From(data.Error).ToIO()
            from _1 in job.AddNotes(notes).ToIO()
            from _2 in job.ChangeStatus(new JobStatus(JobStatusEnum.Failed, currentTime)).ToIO()
            from user in _usersRepository.GetById(job.UserId, cancellationToken)
                .ToIOFailIfNone(Error.New($"User with id: {job.UserId} not found"))
            from avatarsToReturnCount in NaturalNumber.From(1U).ToIO()
            let _3 = user.AddAvatarGenerations(avatarsToReturnCount)
            from _4 in _transactionService.Run(
                from _1 in _jobsRepository.Save(job, cancellationToken)
                from _2 in _usersRepository.Save(user, cancellationToken)
                from _3 in NotifyTelegramUserThatAvatarCreationFailed(job, jobData, cancellationToken)
                //.IfFail(_ => IO<Unit>.Pure(Unit.Default)) - will ignore it when there are multiple profiles
                from _ in DeleteImagesFromObjectStorage(jobData.Input.ImagePaths, cancellationToken)
                select Unit.Default, cancellationToken)
            select Unit.Default;
    }

    private IO<Unit> NotifyTelegramUserThatAvatarCreationFailed(Job job, AvatarCreationJobData _,
        CancellationToken cancellationToken)
    {
        return
            from telegramProfile in _telegramProfilesRepository.GetByUserId(job.UserId, cancellationToken)
                .ToIOFailIfNone(Error.New($"There is no telegram profile for user with id: {job.UserId}"))
            from _1 in _telegramService.ShowAvatarCreationFailedWidget(telegramProfile.ToDto(),
                cancellationToken)
            select Unit.Default;
    }
}