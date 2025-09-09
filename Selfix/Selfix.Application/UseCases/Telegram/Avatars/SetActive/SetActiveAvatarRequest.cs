namespace Selfix.Application.UseCases.Telegram.Avatars.SetActive;

public sealed record SetActiveAvatarRequest(Ulid UserId, Ulid AvatarId);