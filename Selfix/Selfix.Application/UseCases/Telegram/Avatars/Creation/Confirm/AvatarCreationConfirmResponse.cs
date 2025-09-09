namespace Selfix.Application.UseCases.Telegram.Avatars.Creation.Confirm;

public sealed record AvatarCreationConfirmResponse(bool IsStarted, bool LimitedByQuota);