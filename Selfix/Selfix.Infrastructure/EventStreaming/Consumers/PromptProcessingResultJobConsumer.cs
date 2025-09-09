using System.Globalization;
using System.Text.Json;
using MassTransit;
using Selfix.Application.UseCases.Jobs.PromptProcessing;
using Selfix.Schema.Kafka.Jobs.Images.V1.PromptProcessing;
using Selfix.Shared.Extensions;
using Serilog;

namespace Selfix.Infrastructure.EventStreaming.Consumers;

internal sealed class PromptProcessingResultJobConsumer : IConsumer<ProcessPromptResponseEvent>
{
    private readonly IHandlePromptProcessingResultUseCase _handlePromptProcessingResultUseCase;
    private readonly ILogger _logger;

    public PromptProcessingResultJobConsumer(IHandlePromptProcessingResultUseCase handlePromptProcessingResultUseCase,
        ILogger logger)
    {
        _handlePromptProcessingResultUseCase = handlePromptProcessingResultUseCase;
        _logger = logger;
    }

    public Task Consume(ConsumeContext<ProcessPromptResponseEvent> context)
    {
        var id = Ulid.Parse(context.Message.JobId, CultureInfo.InvariantCulture);
        PromptProcessingResultData data = context.Message.IsSuccess
            ? new SucceedPromptProcessingResultData(context.Message.Success!.Prompt)
            : new FailedPromptProcessingResultData(context.Message.Fail!.Error);
        return _handlePromptProcessingResultUseCase
            .Execute(new HandlePromptProcessingResultRequest(id, data), context.CancellationToken)
            .WithLogging(() => _logger.Information("Start processing prompt processing result: {IsSuccess}", context.Message.IsSuccess),
                () => _logger.Information("Finished processing prompt processing result with job id: {JobId}", context.Message.JobId),
                err => _logger.Error("Error processing prompt processing result: {Error}", err))
            .RunAsync()
            .AsTask();
    }
}