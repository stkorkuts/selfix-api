using LanguageExt;
using LanguageExt.Common;
using Selfix.Application.Dtos.Jobs;
using Selfix.Application.ServicesAbstractions.Database;
using Selfix.Application.ServicesAbstractions.Database.Repositories;
using Selfix.Application.ServicesAbstractions.Database.Repositories.Cached;
using Selfix.Application.ServicesAbstractions.Environment;
using Selfix.Application.ServicesAbstractions.EventStreaming;
using Selfix.Application.ServicesAbstractions.Statistics;
using Selfix.Domain.Entities.Jobs;
using Selfix.Domain.Entities.Jobs.Specifications;
using Selfix.Domain.Entities.TelegramProfiles;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Jobs;
using Selfix.Domain.ValueObjects.Jobs.PromptProcessing;
using Selfix.Domain.ValueObjects.Prompts;
using Selfix.Domain.ValueObjects.Quotas;
using Selfix.Domain.ValueObjects.Telegram.Profiles.State;
using Selfix.Shared;
using Selfix.Shared.Extensions;

namespace Selfix.Application.UseCases.Telegram.Prompts.ProcessRawUserPrompt;

internal sealed class ProcessRawUserPromptUseCase : IProcessRawUserPromptUseCase
{
    private readonly IEnvironmentService _environmentService;
    private readonly IEventStreamingService _eventStreamingService;
    private readonly IJobsRepository _jobsRepository;
    private readonly IAvatarsRepository _avatarsRepository;
    private readonly ITelegramProfilesRepository _telegramProfilesRepository;
    private readonly ITransactionService _transactionService;
    private readonly IStatisticsService _statisticsService;
    private readonly IUsersRepository _usersRepository;

    public ProcessRawUserPromptUseCase(CachedTelegramProfilesRepository telegramProfilesRepository,
        IEnvironmentService environmentService, IEventStreamingService eventStreamingService,
        IJobsRepository jobsRepository, IAvatarsRepository avatarsRepository,
        CachedUsersRepository usersRepository, ITransactionService transactionService,
        IStatisticsService statisticsService)
    {
        _telegramProfilesRepository = telegramProfilesRepository;
        _environmentService = environmentService;
        _eventStreamingService = eventStreamingService;
        _jobsRepository = jobsRepository;
        _avatarsRepository = avatarsRepository;
        _usersRepository = usersRepository;
        _transactionService = transactionService;
        _statisticsService = statisticsService;
    }

    public IO<ProcessRawUserPromptResponse> Execute(ProcessRawUserPromptRequest request,
        CancellationToken cancellationToken)
    {
        var profileId = Id<TelegramProfile, long>.FromSafe(request.TelegramProfileId);
        return
            from profile in _telegramProfilesRepository.GetById(profileId, cancellationToken)
                .ToIOFailIfNone(Error.New($"Profile with id: {request.TelegramProfileId} not found"))
            from response in profile.State switch
            {
                TelegramProfileImageGenerationState _ => GeneratePrompt(profile, request.Prompt, cancellationToken),
                _ => Error.New($"Profile with id: {request.TelegramProfileId} is not in image generation state")
            }
            select response;
    }

    private IO<ProcessRawUserPromptResponse> GeneratePrompt(TelegramProfile profile, string prompt,
        CancellationToken cancellationToken)
    {
        return
            from promptTextDomain in PromptText.From(prompt).ToIO()
            from user in _usersRepository.GetById(profile.UserId, cancellationToken)
                .ToIOFailIfNone(Error.New($"User with id: {profile.UserId} not found"))
            from avatar in _avatarsRepository.GetActiveByUserId(user.Id, cancellationToken)
                .ToIOFailIfNone(Error.New($"Active avatar for user with id: {profile.UserId} not found"))
            from userStats in _statisticsService.GetUserStatistics(user.Id, cancellationToken)
            from quotas in UserQuotas.From(user, userStats).ToIO()
            from _1 in quotas.CanGenerateImages() switch
            {
                true => IO<Unit>.Pure(Unit.Default),
                false => Error.New("User exceeded simultaneous image generations quota")
            }
            from _2 in user.RequestImagesGeneration(profile.Settings.ImagesPerRequest).ToIO()
            from currentTime in _environmentService.GetCurrentTime(cancellationToken)
            from seed in _environmentService.GenerateSeed(cancellationToken)
            let jobInput = new PromptProcessingJobInput(avatar.Description, promptTextDomain, profile.Settings.ImagesPerRequest, seed)
            let jobData = new PromptProcessingJobData(jobInput, Option<PromptProcessingJobOutput>.None)
            from job in Job.New(new NewJobSpecification(jobData, currentTime, user.Id)).ToIO()
            from _3 in job.ChangeStatus(new JobStatus(JobStatusEnum.Processing, currentTime)).ToIO()
            from _4 in _transactionService.Run(
                from _1 in _jobsRepository.Save(job, cancellationToken)
                from _2 in _usersRepository.Save(user, cancellationToken)
                from _3 in SendJobForProcessing(job, cancellationToken)
                select Unit.Default, cancellationToken)
            select new ProcessRawUserPromptResponse();
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