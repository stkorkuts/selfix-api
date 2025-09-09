using LanguageExt;
using Selfix.Shared;

namespace Selfix.Application.UseCases.Telegram.Profiles.Update.Settings;

public sealed record UpdateTelegramProfileSettingsRequest(
    long TelegramProfileId,
    Option<uint> TargetImagesPerRequest,
    Option<ImageAspectRatioEnum> TargetImageAspectRatio);