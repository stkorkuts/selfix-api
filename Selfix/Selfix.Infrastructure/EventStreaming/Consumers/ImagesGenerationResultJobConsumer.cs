using System.Globalization;
using System.Text.Json;
using MassTransit;
using Selfix.Application.UseCases.Jobs.ImageGeneration;
using Selfix.Schema.Kafka.Jobs.Images.V1.ImageGeneration;
using Selfix.Shared.Extensions;
using Serilog;

namespace Selfix.Infrastructure.EventStreaming.Consumers;

internal sealed class ImagesGenerationResultJobConsumer : IConsumer<GenerateImageResponseEvent>
{
    private readonly IHandleImageGenerationResultUseCase _handleImageGenerationResultUseCase;
    private readonly ILogger _logger;

    public ImagesGenerationResultJobConsumer(IHandleImageGenerationResultUseCase handleImageGenerationResultUseCase,
        ILogger logger)
    {
        _handleImageGenerationResultUseCase = handleImageGenerationResultUseCase;
        _logger = logger;
    }

    public Task Consume(ConsumeContext<GenerateImageResponseEvent> context)
    {
        var id = Ulid.Parse(context.Message.JobId, CultureInfo.InvariantCulture);
        ImageGenerationResultData data = context.Message.IsSuccess
            ? new SucceedImageGenerationData(context.Message.Success!.GeneratedImagesPaths)
            : new FailedImageGenerationData(context.Message.Fail!.Error);
        return _handleImageGenerationResultUseCase
            .Execute(new HandleImageGenerationResultRequest(id, data), context.CancellationToken)
            .WithLogging(() => _logger.Information("Start processing image generation result: {IsSuccess}", context.Message.IsSuccess),
                () => _logger.Information("Finished processing image generation result with job id: {JobId}", context.Message.JobId),
                err => _logger.Error("Error processing image generation result: {Error}", err))
            .RunAsync()
            .AsTask();
    }
}