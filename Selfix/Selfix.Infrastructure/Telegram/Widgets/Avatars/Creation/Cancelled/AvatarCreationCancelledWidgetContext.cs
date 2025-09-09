using Selfix.Application.Dtos;
using Selfix.Application.Dtos.TelegramProfiles;

namespace Selfix.Infrastructure.Telegram.Widgets.Avatars.Creation.Cancelled;

internal sealed record AvatarCreationCancelledWidgetContext(
    TelegramProfileDto Profile
) : WidgetContext(Profile);