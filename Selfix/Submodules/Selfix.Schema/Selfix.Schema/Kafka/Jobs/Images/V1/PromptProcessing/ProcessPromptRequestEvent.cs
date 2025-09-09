namespace Selfix.Schema.Kafka.Jobs.Images.V1.PromptProcessing;

public sealed class ProcessPromptRequestEvent
{
    public required string JobId { get; init; }
    public required long Seed { get; init; }
    public required string AvatarDescription { get; init; }
    public required string RawPrompt { get; init; }
}