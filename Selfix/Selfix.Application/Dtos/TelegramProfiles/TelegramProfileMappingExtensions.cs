using Selfix.Domain.Entities.TelegramProfiles;
using Selfix.Domain.ValueObjects.Telegram.Profiles.State;
using Selfix.Shared;

namespace Selfix.Application.Dtos.TelegramProfiles;

internal static class TelegramProfileMappingExtensions
{
    public static TelegramProfileDto ToDto(this TelegramProfile profile)
    {
        return new TelegramProfileDto(
            profile.Id,
            new TelegramProfileDto.SettingsDto(profile.Settings.ImagesPerRequest, profile.Settings.ImageAspectRatio),
            profile.State switch
            {
                TelegramProfileAvatarCreationState => TelegramProfileStateEnum.AvatarCreation,
                TelegramProfileImageGenerationState => TelegramProfileStateEnum.ImageGeneration,
                TelegramProfileDefaultState => TelegramProfileStateEnum.Default,
                _ => TelegramProfileStateEnum.Default
            }
        );
    }
}