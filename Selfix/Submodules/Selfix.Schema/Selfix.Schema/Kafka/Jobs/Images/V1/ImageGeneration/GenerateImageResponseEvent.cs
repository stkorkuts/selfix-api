namespace Selfix.Schema.Kafka.Jobs.Images.V1.ImageGeneration;

public sealed class GenerateImageResponseEvent
{
    public required string JobId { get; init; }
    public required bool IsSuccess { get; init; }
    public GenerateImageResponseEventSuccessData? Success { get; init; }
    public GenerateImageResponseEventFailData? Fail { get; init; }
}