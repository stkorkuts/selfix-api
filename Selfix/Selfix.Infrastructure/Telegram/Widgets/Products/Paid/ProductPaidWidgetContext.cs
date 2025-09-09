using Selfix.Application.Dtos;
using Selfix.Application.Dtos.Products;
using Selfix.Application.Dtos.TelegramProfiles;

namespace Selfix.Infrastructure.Telegram.Widgets.Products.Paid;

internal sealed record ProductPaidWidgetContext(TelegramProfileDto Profile, ProductDto Product) : WidgetContext(Profile);