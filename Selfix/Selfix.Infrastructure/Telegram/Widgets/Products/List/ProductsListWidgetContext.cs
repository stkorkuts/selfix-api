using LanguageExt;
using Selfix.Application.Dtos;
using Selfix.Application.Dtos.Products;
using Selfix.Application.Dtos.TelegramProfiles;
using Selfix.Application.Dtos.Users;

namespace Selfix.Infrastructure.Telegram.Widgets.Products.List;

internal sealed record ProductsListWidgetContext(
    TelegramProfileDto Profile,
    UserDto User,
    Iterable<ProductDto> Products,
    bool IsPromocode) : WidgetContext(Profile);