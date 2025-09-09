using LanguageExt;
using LanguageExt.Common;
using Selfix.Application.Dtos.Jobs;
using Selfix.Application.ServicesAbstractions.Database;
using Selfix.Application.ServicesAbstractions.Database.Repositories;
using Selfix.Application.ServicesAbstractions.Database.Repositories.Cached;
using Selfix.Application.ServicesAbstractions.Environment;
using Selfix.Application.ServicesAbstractions.EventStreaming;
using Selfix.Application.ServicesAbstractions.ObjectStorage;
using Selfix.Application.ServicesAbstractions.Statistics;
using Selfix.Application.ServicesAbstractions.Telegram;
using Selfix.Domain.Entities.Jobs;
using Selfix.Domain.Entities.Jobs.Specifications;
using Selfix.Domain.Entities.TelegramProfiles;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Jobs;
using Selfix.Domain.ValueObjects.Jobs.AvatarCreation;
using Selfix.Domain.ValueObjects.ObjectStorage;
using Selfix.Domain.ValueObjects.Quotas;
using Selfix.Domain.ValueObjects.Telegram.Files;
using Selfix.Domain.ValueObjects.Telegram.Profiles.State;
using Selfix.Shared;
using Selfix.Shared.Extensions;

namespace Selfix.Application.UseCases.Telegram.Avatars.Creation.Confirm;

internal sealed class AvatarCreationConfirmUseCase : IAvatarCreationConfirmUseCase
{
    private readonly IEnvironmentService _environmentService;
    private readonly IEventStreamingService _eventStreamingService;
    private readonly IJobsRepository _jobsRepository;
    private readonly IStatisticsService _statisticsService;
    private readonly IObjectStorageService _objectStorageService;
    private readonly ITelegramProfilesRepository _telegramProfilesRepository;
    private readonly ITelegramService _telegramService;
    private readonly ITransactionService _transactionService;
    private readonly IUsersRepository _usersRepository;

    public AvatarCreationConfirmUseCase(CachedTelegramProfilesRepository telegramProfilesRepository,
        IEventStreamingService eventStreamingService, ITelegramService telegramService,
        IObjectStorageService objectStorageService, IEnvironmentService environmentService,
        IJobsRepository jobsRepository, IStatisticsService statisticsService,
        CachedUsersRepository usersRepository, ITransactionService transactionService)
    {
        _telegramProfilesRepository = telegramProfilesRepository;
        _eventStreamingService = eventStreamingService;
        _telegramService = telegramService;
        _objectStorageService = objectStorageService;
        _environmentService = environmentService;
        _jobsRepository = jobsRepository;
        _statisticsService = statisticsService;
        _usersRepository = usersRepository;
        _transactionService = transactionService;
    }

    public IO<AvatarCreationConfirmResponse> Execute(AvatarCreationConfirmRequest request,
        CancellationToken cancellationToken)
    {
        var profileId = Id<TelegramProfile, long>.FromSafe(request.TelegramProfileId);
        return
            from profile in _telegramProfilesRepository.GetById(profileId, cancellationToken)
                .ToIOFailIfNone(Error.New($"Profile with id: {request.TelegramProfileId} not found"))
            from response in profile.State switch
            {
                TelegramProfileAvatarCreationState state => ConfirmAvatarCreation(profile, state,
                    cancellationToken),
                _ => Error.New("Profile is not in avatar creation state")
            }
            select response;
    }

    private IO<AvatarCreationConfirmResponse> ConfirmAvatarCreation(TelegramProfile profile,
        TelegramProfileAvatarCreationState state,
        CancellationToken cancellationToken)
    {
        return
            from user in _usersRepository.GetById(profile.UserId, cancellationToken)
                .ToIOFailIfNone(Error.New($"User with id: {profile.UserId} not found"))
            from avatarStateData in state.CheckReadyForCreation().ToIO()
            from userStats in _statisticsService.GetUserStatistics(user.Id, cancellationToken)
            from quotas in UserQuotas.From(user, userStats).ToIO()
            from _1 in quotas.CanGenerateAvatars() switch
            {
                true => IO<Unit>.Pure(Unit.Default),
                false => Error.New("User exceeded simultaneous avatar generations quota")
            }
            from _2 in user.RequestAvatarCreation().ToIO()
            from fileLocations in TransferImagesFromTelegramToObjectStorage(avatarStateData.FileIds, cancellationToken)
            let jobInput = new AvatarCreationJobInput(fileLocations.Map(l => l.FilePath), avatarStateData.Name)
            let jobData = new AvatarCreationJobData(jobInput, Option<AvatarCreationJobOutput>.None)
            from currentTime1 in _environmentService.GetCurrentTime(cancellationToken)
            from job in Job.New(new NewJobSpecification(jobData, currentTime1, user.Id)).ToIO()
            from _3 in _eventStreamingService.SendJobForProcessing(job.ToDto(), cancellationToken)
                .InterceptFail(
                    err =>
                        from _1 in RemoveFilesFromObjectStorage(fileLocations, cancellationToken)
                        from currentTime in _environmentService.GetCurrentTime(cancellationToken)
                        from _2 in job.ChangeStatus(new JobStatus(JobStatusEnum.Failed, currentTime)).ToIO()
                        from notes in Notes.From(err.ToString()).ToIO()
                        from _3 in job.AddNotes(notes).ToIO()
                        from _4 in _jobsRepository.Save(job, cancellationToken)
                        select Unit.Default
                )
            from currentTime2 in _environmentService.GetCurrentTime(cancellationToken)
            from _4 in job.ChangeStatus(new JobStatus(JobStatusEnum.Processing, currentTime2)).ToIO()
            from _5 in profile.ChangeState(TelegramProfileDefaultState.New()).ToIO()
            from _6 in _transactionService.Run(
                from _1 in _jobsRepository.Save(job, cancellationToken)
                from _2 in _telegramProfilesRepository.Save(profile, cancellationToken)
                from _3 in _usersRepository.Save(user, cancellationToken)
                select Unit.Default, cancellationToken)
            select new AvatarCreationConfirmResponse(true, false);
    }

    private IO<Iterable<OSFileLocation>> TransferImagesFromTelegramToObjectStorage(Iterable<TelegramFile> files,
        CancellationToken cancellationToken)
    {
        return
            from bucket in _objectStorageService.GetBucketName(BucketContentTypeEnum.Temporary)
            from locations in files.TraverseM(f => MoveFileFromTelegramToObjectStorage(bucket, f, cancellationToken))
            select locations;
    }

    private IO<OSFileLocation> MoveFileFromTelegramToObjectStorage(OSBucketName bucket, TelegramFile tgFile,
        CancellationToken cancellationToken)
    {
        return
            from stream in IO<Stream>.Lift(() => new MemoryStream())
            from location in (
                from _1 in _telegramService.PutFileIntoStream(tgFile, stream, cancellationToken)
                from filePath in OSFilePath.New(string.Empty, tgFile.Extension).ToIO()
                let fileLocation = OSFileLocation.Create(bucket, filePath)
                from _2 in _objectStorageService.UploadFile(stream, fileLocation, cancellationToken)
                select fileLocation).Finally(IO<Unit>.Lift(() =>
                {
                    stream.Dispose();
                    return Unit.Default;
                })
            )
            select location;
    }

    private IO<Unit> RemoveFilesFromObjectStorage(Iterable<OSFileLocation> fileLocations,
        CancellationToken cancellationToken)
    {
        return
            from _1 in fileLocations.TraverseM(fileLocation =>
                _objectStorageService.DeleteFileIfExist(fileLocation, cancellationToken)).As()
            select Unit.Default;
    }
}