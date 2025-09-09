namespace Selfix.Schema.Kafka.Jobs.Avatars.V1.AvatarCreation;

public sealed class CreateAvatarResponseEventSuccessData
{
    public required string AvatarPath { get; init; }
    public required string AvatarDescription { get; init; }
}