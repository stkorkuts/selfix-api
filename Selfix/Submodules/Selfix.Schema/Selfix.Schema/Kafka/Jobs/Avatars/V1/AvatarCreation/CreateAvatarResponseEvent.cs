namespace Selfix.Schema.Kafka.Jobs.Avatars.V1.AvatarCreation;

public sealed class CreateAvatarResponseEvent
{
    public required string JobId { get; init; }
    public required bool IsSuccess { get; init; }
    public CreateAvatarResponseEventSuccessData? Success { get; init; }
    public CreateAvatarResponseEventFailData? Fail { get; init; }
}