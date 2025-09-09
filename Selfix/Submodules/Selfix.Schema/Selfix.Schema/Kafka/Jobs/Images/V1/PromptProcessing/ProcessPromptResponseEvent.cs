namespace Selfix.Schema.Kafka.Jobs.Images.V1.PromptProcessing;

public sealed class ProcessPromptResponseEvent
{
    public required string JobId { get; init; }
    public required bool IsSuccess { get; init; }
    public ProcessPromptResponseEventSuccessData? Success { get; init; }
    public ProcessPromptResponseEventFailData? Fail { get; init; }
}