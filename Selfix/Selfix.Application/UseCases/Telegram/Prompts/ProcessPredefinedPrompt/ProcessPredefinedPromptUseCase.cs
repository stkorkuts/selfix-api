using LanguageExt;
using LanguageExt.Common;
using Selfix.Application.Dtos.Jobs;
using Selfix.Application.ServicesAbstractions.Database;
using Selfix.Application.ServicesAbstractions.Database.Repositories;
using Selfix.Application.ServicesAbstractions.Database.Repositories.Cached;
using Selfix.Application.ServicesAbstractions.Environment;
using Selfix.Application.ServicesAbstractions.EventStreaming;
using Selfix.Application.ServicesAbstractions.Statistics;
using Selfix.Application.UseCases.Telegram.Prompts.ProcessRawUserPrompt;
using Selfix.Domain.Entities.Jobs;
using Selfix.Domain.Entities.Jobs.Specifications;
using Selfix.Domain.Entities.Prompts;
using Selfix.Domain.Entities.TelegramProfiles;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Jobs;
using Selfix.Domain.ValueObjects.Jobs.ImageGeneration;
using Selfix.Domain.ValueObjects.Jobs.PromptProcessing;
using Selfix.Domain.ValueObjects.Prompts;
using Selfix.Domain.ValueObjects.Quotas;
using Selfix.Shared;
using Selfix.Shared.Extensions;

namespace Selfix.Application.UseCases.Telegram.Images.GenerateByPredefinedPrompt;

internal sealed class ProcessPredefinedPromptUseCase : IProcessPredefinedPromptUseCase
{
    private readonly IEnvironmentService _environmentService;
    private readonly IEventStreamingService _eventStreamingService;
    private readonly IJobsRepository _jobsRepository;
    private readonly IAvatarsRepository _avatarsRepository;
    private readonly IPromptsRepository _promptsRepository;
    private readonly ITelegramProfilesRepository _telegramProfilesRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly ITransactionService _transactionService;
    private readonly IStatisticsService _statisticsService;

    public ProcessPredefinedPromptUseCase(CachedTelegramProfilesRepository telegramProfilesRepository,
        IEnvironmentService environmentService, IEventStreamingService eventStreamingService,
        IJobsRepository jobsRepository, IAvatarsRepository avatarsRepository,
        CachedPromptsRepository promptsRepository, CachedUsersRepository usersRepository,
        ITransactionService transactionService, IStatisticsService statisticsService)
    {
        _telegramProfilesRepository = telegramProfilesRepository;
        _environmentService = environmentService;
        _eventStreamingService = eventStreamingService;
        _jobsRepository = jobsRepository;
        _avatarsRepository = avatarsRepository;
        _promptsRepository = promptsRepository;
        _usersRepository = usersRepository;
        _transactionService = transactionService;
        _statisticsService = statisticsService;
    }

    public IO<ProcessPredefinedPromptResponse> Execute(ProcessPredefinedPromptRequest request,
        CancellationToken cancellationToken)
    {
        var profileId = Id<TelegramProfile, long>.FromSafe(request.TelegramProfileId);
        return
            from profile in _telegramProfilesRepository.GetById(profileId, cancellationToken)
                .ToIOFailIfNone(Error.New($"Profile with id: {request.TelegramProfileId} not found"))
            from user in _usersRepository.GetById(profile.UserId, cancellationToken)
                .ToIOFailIfNone(Error.New($"User with id: {profile.UserId} not found"))
            from activeAvatar in _avatarsRepository.GetActiveByUserId(user.Id, cancellationToken)
                .ToIOFailIfNone(Error.New($"Active avatar for user with id {user.Id} not found"))
            from userStats in _statisticsService.GetUserStatistics(user.Id, cancellationToken)
            from quotas in UserQuotas.From(user, userStats).ToIO()
            from _1 in quotas.CanGenerateImages() switch
            {
                true => IO<Unit>.Pure(Unit.Default),
                false => Error.New("User exceeded simultaneous image generations quota")
            }
            from _2 in user.RequestImagesGeneration(profile.Settings.ImagesPerRequest).ToIO()
            from predefinedPrompt in _promptsRepository.GetById(Id<Prompt, Ulid>.FromSafe(request.PredefinedPromptId),
                    cancellationToken)
                .ToIOFailIfNone(Error.New($"Predefined prompt with id: {request.PredefinedPromptId} not found"))
            from promptTextDomain in PromptText.From(predefinedPrompt.Text).ToIO()
            from currentTime in _environmentService.GetCurrentTime(cancellationToken)
            from seed in _environmentService.GenerateSeed(cancellationToken)
            let jobInput = new PromptProcessingJobInput(activeAvatar.Description, promptTextDomain, profile.Settings.ImagesPerRequest, seed)
            let jobData = new PromptProcessingJobData(jobInput, Option<PromptProcessingJobOutput>.None)
            from job in Job.New(new NewJobSpecification(jobData, currentTime, user.Id)).ToIO()
            from _3 in job.ChangeStatus(new JobStatus(JobStatusEnum.Processing, currentTime)).ToIO()
            from _4 in _transactionService.Run(
                from _1 in _jobsRepository.Save(job, cancellationToken)
                from _2 in _usersRepository.Save(user, cancellationToken)
                from _3 in SendJobForProcessing(job, cancellationToken)
                select Unit.Default, cancellationToken)
            select new ProcessPredefinedPromptResponse();
    }
    
    private IO<Unit> SendJobForProcessing(Job job, CancellationToken cancellationToken)
    {
        return _eventStreamingService.SendJobForProcessing(job.ToDto(), cancellationToken)
            .InterceptFail(err =>
                from currentTime in _environmentService.GetCurrentTime(cancellationToken)
                from _1 in job.ChangeStatus(new JobStatus(JobStatusEnum.Failed, currentTime)).ToIO()
                from notes in Notes.From(err.ToString()).ToIO()
                from _2 in job.AddNotes(notes).ToIO()
                from _3 in _jobsRepository.Save(job, cancellationToken)
                select Unit.Default
            );
    }
}