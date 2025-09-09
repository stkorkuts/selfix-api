using Selfix.Application.Dtos;
using Selfix.Application.Dtos.TelegramProfiles;

namespace Selfix.Infrastructure.Telegram.Widgets.Avatars.Creation.ImageAdded;

internal sealed record AvatarCreationImageAddedWidgetContext(
    TelegramProfileDto Profile,
    uint TotalImagesLoaded
) : WidgetContext(Profile);