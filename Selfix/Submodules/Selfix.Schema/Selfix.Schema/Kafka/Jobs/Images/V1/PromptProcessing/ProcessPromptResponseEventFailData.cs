namespace Selfix.Schema.Kafka.Jobs.Images.V1.PromptProcessing;

public sealed class ProcessPromptResponseEventFailData
{
    public required string Error { get; init; }
}