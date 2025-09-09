namespace Selfix.Schema.Kafka.Jobs.Images.V1.ImageGeneration;

public sealed class GenerateImageRequestEvent
{
    public required string JobId { get; init; }
    public required string AvatarLoraPath { get; init; }
    public required int Quantity { get; init; }
    public required long Seed { get; init; }
    public required string Prompt { get; init; }
    public required ImageAspectRatio AspectRatio { get; set; }
}