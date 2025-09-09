namespace Selfix.Schema.Kafka.Jobs.Images.V1.PromptProcessing;

public sealed class ProcessPromptResponseEventSuccessData
{
    public required string Prompt { get; init; }
}