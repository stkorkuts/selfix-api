namespace Selfix.Application.UseCases.Telegram.Avatars.Creation.AddImage;

public sealed record AvatarCreationAddImageRequest(long TelegramProfileId, string FileId, string FileExtension);