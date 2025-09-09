namespace Selfix.Schema.Kafka.Jobs.Avatars.V1.AvatarCreation;

public sealed class CreateAvatarRequestEvent
{
    public required string JobId { get; init; }
    public required string[] SourceImagesPaths { get; init; }
}