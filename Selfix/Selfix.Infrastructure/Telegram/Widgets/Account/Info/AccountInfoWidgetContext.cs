using Selfix.Application.Dtos;
using Selfix.Application.Dtos.TelegramProfiles;
using Selfix.Application.Dtos.Users;

namespace Selfix.Infrastructure.Telegram.Widgets.Account.Info;

internal sealed record AccountInfoWidgetContext(
    TelegramProfileDto Profile,
    UserDto User
) : WidgetContext(Profile);