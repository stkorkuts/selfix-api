using Selfix.Shared;

namespace Selfix.Application.Dtos.TelegramProfiles;

public sealed record TelegramProfileDto(
    long Id,
    TelegramProfileDto.SettingsDto Settings,
    TelegramProfileStateEnum ProfileState)
{
    public sealed record SettingsDto(uint ImagesToGeneratePerRequest, ImageAspectRatioEnum ImageAspectRatio);

    public sealed record UserDataDto(
        Ulid UserId,
        uint AvailableAvatarGenerations,
        uint AvailableImageGenerations,
        bool HasPayments);
}