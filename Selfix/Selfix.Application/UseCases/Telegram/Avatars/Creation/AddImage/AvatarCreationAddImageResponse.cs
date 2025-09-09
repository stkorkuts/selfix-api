namespace Selfix.Application.UseCases.Telegram.Avatars.Creation.AddImage;

public sealed record AvatarCreationAddImageResponse(
    uint TotalImagesUploaded,
    bool CanStartGeneration,
    bool IsImageIgnored);