using Selfix.Application.Dtos;
using Selfix.Application.Dtos.Avatars;
using Selfix.Application.Dtos.TelegramProfiles;

namespace Selfix.Infrastructure.Telegram.Widgets.Avatars.Activated;

internal sealed record AvatarActivatedWidgetContext(
    TelegramProfileDto Profile,
    AvatarDto Avatar
) : WidgetContext(Profile);