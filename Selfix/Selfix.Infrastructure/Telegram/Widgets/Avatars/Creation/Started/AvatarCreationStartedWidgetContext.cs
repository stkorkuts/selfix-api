using Selfix.Application.Dtos;
using Selfix.Application.Dtos.TelegramProfiles;

namespace Selfix.Infrastructure.Telegram.Widgets.Avatars.Creation.Started;

internal sealed record AvatarCreationStartedWidgetContext(
    TelegramProfileDto Profile
) : WidgetContext(Profile);