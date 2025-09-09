using Selfix.Application.Dtos;
using Selfix.Application.Dtos.TelegramProfiles;

namespace Selfix.Infrastructure.Telegram.Widgets.Help;

internal sealed record HelpWidgetContext(
    TelegramProfileDto Profile,
    Uri HelpCenterUrl
) : WidgetContext(Profile);