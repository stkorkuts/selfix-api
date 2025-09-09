using Selfix.Application.Dtos;
using Selfix.Application.Dtos.TelegramProfiles;

namespace Selfix.Infrastructure.Telegram.Widgets.Notifications.Simple;

internal sealed record SimpleNotificationWidgetContext(TelegramProfileDto Profile, string Text) : WidgetContext(Profile);