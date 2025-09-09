using Selfix.Application.Dtos;
using Selfix.Application.Dtos.Promocodes;
using Selfix.Application.Dtos.TelegramProfiles;

namespace Selfix.Infrastructure.Telegram.Widgets.Promocodes.Paid;

internal sealed record PromocodePaidWidgetContext(TelegramProfileDto Profile, PromocodeDto Promocode) : WidgetContext(Profile);