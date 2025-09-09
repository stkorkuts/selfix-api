namespace Selfix.Schema.Kafka.Jobs.Images.V1.ImageGeneration;

public sealed class GenerateImageResponseEventSuccessData
{
    public required string[] GeneratedImagesPaths { get; init; }
}