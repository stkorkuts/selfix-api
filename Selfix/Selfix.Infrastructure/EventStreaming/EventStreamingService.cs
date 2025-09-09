using LanguageExt;
using MassTransit;
using Microsoft.Extensions.Options;
using Selfix.Application.Dtos;
using Selfix.Application.Dtos.Jobs;
using Selfix.Application.Dtos.TelegramProfiles;
using Selfix.Application.ServicesAbstractions.Environment;
using Selfix.Application.ServicesAbstractions.EventStreaming;
using Selfix.Domain.ValueObjects.Jobs.AvatarCreation;
using Selfix.Domain.ValueObjects.Jobs.ImageGeneration;
using Selfix.Domain.ValueObjects.Jobs.PromptProcessing;
using Selfix.Schema.Kafka.Jobs.Avatars.V1.AvatarCreation;
using Selfix.Schema.Kafka.Jobs.Images.V1.ImageGeneration;
using Selfix.Schema.Kafka.Jobs.Images.V1.PromptProcessing;
using Selfix.Schema.Kafka.Notifications.V1;
using Selfix.Shared.Extensions;
using Selfix.Shared.Settings;

namespace Selfix.Infrastructure.EventStreaming;

internal sealed class EventStreamingService : IEventStreamingService
{
    private readonly ITopicProducer<CreateAvatarRequestEvent> _createAvatarProducer;
    private readonly ITopicProducer<GenerateImageRequestEvent> _generateImageProducer;
    private readonly ITopicProducer<ProcessPromptRequestEvent> _processImagePromptProducer;
    private readonly IEnvironmentService _environmentService;
    private readonly EventStreamingSettings _eventStreamingSettings;

    public EventStreamingService(ITopicProducer<ProcessPromptRequestEvent> processImagePromptProducer,
        ITopicProducer<GenerateImageRequestEvent> generateImageProducer,
        ITopicProducer<CreateAvatarRequestEvent> createAvatarProducer,
        IEnvironmentService environmentService,
        IOptions<EventStreamingSettings> eventStreamingOptions)
    {
        _processImagePromptProducer = processImagePromptProducer;
        _generateImageProducer = generateImageProducer;
        _createAvatarProducer = createAvatarProducer;
        _environmentService = environmentService;
        _eventStreamingSettings = eventStreamingOptions.Value;
    }

    public IO<Unit> SendJobForProcessing(JobDto job, CancellationToken cancellationToken)
    {
        return (job.Data switch
        {
            AvatarCreationJobData data => _createAvatarProducer.Produce(new CreateAvatarRequestEvent
            {
                JobId = job.Id.ToString(),
                SourceImagesPaths = data.Input.ImagePaths.Map(path => (string)path).ToArray()
            }, cancellationToken),
            ImageGenerationJobData data => _generateImageProducer.Produce(new GenerateImageRequestEvent
            {
                JobId = job.Id.ToString(),
                AvatarLoraPath = data.Input.AvatarLoraPath,
                Prompt = data.Input.Prompt,
                Quantity = data.Input.Quantity,
                Seed = data.Input.Seed,
                AspectRatio = data.Input.AspectRatio.ToKafkaSchema()
            }, cancellationToken),
            PromptProcessingJobData data => _processImagePromptProducer.Produce(new ProcessPromptRequestEvent
            {
                JobId = job.Id.ToString(),
                AvatarDescription = data.Input.AvatarDescription,
                RawPrompt = data.Input.RawPrompt,
                Seed = data.Input.Seed
            }, cancellationToken),
            _ => Task.FromException(new NotImplementedException($"Job type {job.Data.GetType()} is not supported"))
        }).ToIO();
    }
}