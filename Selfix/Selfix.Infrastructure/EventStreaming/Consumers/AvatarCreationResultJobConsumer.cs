using System.Globalization;
using System.Text.Json;
using MassTransit;
using Selfix.Application.UseCases.Jobs.AvatarCreation;
using Selfix.Schema.Kafka.Jobs.Avatars.V1.AvatarCreation;
using Selfix.Shared.Extensions;
using Serilog;

namespace Selfix.Infrastructure.EventStreaming.Consumers;

internal sealed class AvatarCreationResultJobConsumer : IConsumer<CreateAvatarResponseEvent>
{
    private readonly IHandleAvatarCreationResultUseCase _handleAvatarCreationResultUseCase;
    private readonly ILogger _logger;

    public AvatarCreationResultJobConsumer(IHandleAvatarCreationResultUseCase handleAvatarCreationResultUseCase,
        ILogger logger)
    {
        _handleAvatarCreationResultUseCase = handleAvatarCreationResultUseCase;
        _logger = logger;
    }

    public Task Consume(ConsumeContext<CreateAvatarResponseEvent> context)
    {
        var id = Ulid.Parse(context.Message.JobId, CultureInfo.InvariantCulture);
        AvatarCreationResultData data = context.Message.IsSuccess
            ? new SucceedAvatarCreationData(context.Message.Success!.AvatarDescription, context.Message.Success!.AvatarPath)
            : new FailedAvatarCreationData(context.Message.Fail!.Error);
        return _handleAvatarCreationResultUseCase
            .Execute(new HandleAvatarCreationResultRequest(id, data), context.CancellationToken)
            .WithLogging(() => _logger.Information("Start processing avatar creation result: {IsSuccess}", context.Message.IsSuccess),
                () => _logger.Information("Finished processing avatar creation result with job id: {JobId}", context.Message.JobId),
                err => _logger.Error("Error processing avatar creation result: {Error}", err))
            .RunAsync()
            .AsTask();
    }
}