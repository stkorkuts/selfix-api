using Selfix.Application.Dtos;
using Selfix.Application.Dtos.TelegramProfiles;

namespace Selfix.Infrastructure.Telegram.Widgets.Promocodes.ApplicationResult;

internal sealed record PromocodeApplicationResultWidgetContext(
    TelegramProfileDto Profile,
    bool IsApplied
) : WidgetContext(Profile);