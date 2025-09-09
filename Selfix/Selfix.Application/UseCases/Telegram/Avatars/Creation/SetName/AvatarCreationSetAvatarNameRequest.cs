namespace Selfix.Application.UseCases.Telegram.Avatars.Creation.SetName;

public sealed record AvatarCreationSetAvatarNameRequest(long TelegramProfileId, string Name);