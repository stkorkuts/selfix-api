using LanguageExt;
using LanguageExt.Common;
using Selfix.Application.Dtos.Jobs;
using Selfix.Application.Dtos.TelegramProfiles;
using Selfix.Application.ServicesAbstractions.Database;
using Selfix.Application.ServicesAbstractions.Database.Repositories;
using Selfix.Application.ServicesAbstractions.Database.Repositories.Cached;
using Selfix.Application.ServicesAbstractions.Environment;
using Selfix.Application.ServicesAbstractions.EventStreaming;
using Selfix.Application.ServicesAbstractions.Telegram;
using Selfix.Domain.Entities.Jobs;
using Selfix.Domain.Entities.Jobs.Specifications;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Jobs;
using Selfix.Domain.ValueObjects.Jobs.ImageGeneration;
using Selfix.Domain.ValueObjects.Jobs.PromptProcessing;
using Selfix.Domain.ValueObjects.Prompts;
using Selfix.Shared;
using Selfix.Shared.Extensions;

namespace Selfix.Application.UseCases.Jobs.PromptProcessing;

internal sealed class HandlePromptProcessingResultUseCase : IHandlePromptProcessingResultUseCase
{
    private readonly IEnvironmentService _environmentService;
    private readonly IEventStreamingService _eventStreamingService;
    private readonly IAvatarsRepository _avatarsRepository;
    private readonly IJobsRepository _jobsRepository;
    private readonly ITelegramProfilesRepository _telegramProfilesRepository;
    private readonly ITelegramService _telegramService;
    private readonly ITransactionService _transactionService;
    private readonly IUsersRepository _usersRepository;

    public HandlePromptProcessingResultUseCase(IJobsRepository jobsRepository, IEnvironmentService environmentService,
        CachedUsersRepository usersRepository, CachedTelegramProfilesRepository telegramProfilesRepository,
        ITransactionService transactionService, ITelegramService telegramService,
        IEventStreamingService eventStreamingService, IAvatarsRepository avatarsRepository)
    {
        _jobsRepository = jobsRepository;
        _environmentService = environmentService;
        _usersRepository = usersRepository;
        _telegramProfilesRepository = telegramProfilesRepository;
        _transactionService = transactionService;
        _telegramService = telegramService;
        _eventStreamingService = eventStreamingService;
        _avatarsRepository = avatarsRepository;
    }

    public IO<HandlePromptProcessingResultResponse> Execute(HandlePromptProcessingResultRequest request,
        CancellationToken cancellationToken)
    {
        return
            from job in _jobsRepository.GetById(Id<Job, Ulid>.FromSafe(request.JobId), cancellationToken)
                .ToIOFailIfNone(Error.New($"Job with id {request.JobId} not found"))
            from _1 in request.Data switch
            {
                SucceedPromptProcessingResultData data => HandlePromptProcessingSuccess(job, data, cancellationToken),
                FailedPromptProcessingResultData data => HandlePromptProcessingFail(job, data, cancellationToken),
                _ => Error.New($"There is no handler for data type: {request.Data.GetType()}")
            }
            select new HandlePromptProcessingResultResponse();
    }

    private IO<Unit> HandlePromptProcessingSuccess(Job promptProcessingJob, SucceedPromptProcessingResultData data,
        CancellationToken cancellationToken)
    {
        return
            from activeAvatar in _avatarsRepository.GetActiveByUserId(promptProcessingJob.UserId, cancellationToken)
                .ToIOFailIfNone(Error.New($"Active avatar for user with id {promptProcessingJob.UserId} not found"))
            from profile in _telegramProfilesRepository.GetByUserId(promptProcessingJob.UserId, cancellationToken)
                .ToIOFailIfNone(Error.New($"Profile with user id: {promptProcessingJob.UserId} not found"))
            from currentTime in _environmentService.GetCurrentTime(cancellationToken)
            from processedPrompt in PromptText.From(data.ProcessedPrompt).ToIO()
            from promptProcessingJobData in promptProcessingJob
                .SetOutput(new PromptProcessingJobOutput(processedPrompt)).ToIO()
            from _1 in promptProcessingJob.ChangeStatus(new JobStatus(JobStatusEnum.Succeeded, currentTime)).ToIO()
            from seed in _environmentService.GenerateSeed(cancellationToken)
            let imageGenerationJobInput =
                new ImageGenerationJobInput(activeAvatar.OSFilePath, processedPrompt, promptProcessingJobData.Input.Quantity, seed, profile.Settings.ImageAspectRatio)
            let imageGenerationJobData =
                new ImageGenerationJobData(imageGenerationJobInput, Option<ImageGenerationJobOutput>.None)
            from imageGenerationJob in Job
                .New(new NewJobSpecification(imageGenerationJobData, currentTime, promptProcessingJob.UserId)).ToIO()
            from _2 in _transactionService.Run(
                from _1 in _jobsRepository.Save(promptProcessingJob, cancellationToken)
                from _2 in _jobsRepository.Save(imageGenerationJob, cancellationToken)
                from _3 in _eventStreamingService.SendJobForProcessing(imageGenerationJob.ToDto(),
                    cancellationToken)
                select Unit.Default, cancellationToken)
            select Unit.Default;
    }

    private IO<Unit> HandlePromptProcessingFail(Job job, FailedPromptProcessingResultData data,
        CancellationToken cancellationToken)
    {
        return
            from currentTime in _environmentService.GetCurrentTime(cancellationToken)
            from jobData in job.Data switch
            {
                PromptProcessingJobData jobData =>
                    from notes in Notes.From(data.Error).ToIO()
                    from _1 in job.AddNotes(notes).ToIO()
                    from _2 in job.ChangeStatus(new JobStatus(JobStatusEnum.Failed, currentTime)).ToIO()
                    from user in _usersRepository.GetById(job.UserId, cancellationToken)
                        .ToIOFailIfNone(Error.New($"User with id: {job.UserId} not found"))
                    let _3 = user.AddImageGenerations(jobData.Input.Quantity)
                    from _4 in _transactionService.Run(
                        from _1 in _jobsRepository.Save(job, cancellationToken)
                        from _2 in _usersRepository.Save(user, cancellationToken)
                        from _3 in NotifyTelegramUserThatImagesGenerationFailed(job, cancellationToken)
                        //.IfFail(_ => IO<Unit>.Pure(Unit.Default)) - will ignore it when there are multiple profiles
                        select Unit.Default, cancellationToken)
                    select Unit.Default,
                _ => Error.New($"There is no handler for data type: {job.Data.GetType()}")
            }
            select Unit.Default;
    }

    private IO<Unit> NotifyTelegramUserThatImagesGenerationFailed(Job job, CancellationToken cancellationToken)
    {
        return
            from telegramProfile in _telegramProfilesRepository.GetByUserId(job.UserId, cancellationToken)
                .ToIOFailIfNone(Error.New($"There is no telegram profile for user with id: {job.UserId}"))
            from _1 in _telegramService.ShowImageGenerationFailedWidget(telegramProfile.ToDto(), cancellationToken)
            select Unit.Default;
    }
}