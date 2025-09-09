using LanguageExt;
using Selfix.Domain.ValueObjects.Products.Packages;
using Selfix.Shared.Extensions;
using Telegram.Bot;

namespace Selfix.Infrastructure.Telegram.Widgets.Products.Paid;

internal sealed class ProductPaidWidget : IWidget<ProductPaidWidgetContext>
{
    private readonly ITelegramBotClient _client;

    public ProductPaidWidget(ITelegramBotClient client)
    {
        _client = client;
    }
    
    public IO<Unit> Show(ProductPaidWidgetContext context, CancellationToken cancellationToken)
    {
        var message = context.Product.Data switch
        {
            PackageProductData packageData => packageData switch
            {
                {AvatarGenerationsCount: > 0} => "Пакет куплен! Теперь вы можете создать свой аватар и начать творить!",
                {ImageGenerationsCount: > 0} => "Пакет куплен! Теперь вы можете создавать ещё больше крутых изображений!",
                _ => "Пакет куплен!"
            },
            _ => "Пакет куплен!"
        };
        return
            _client.SendMessage(context.Profile.Id, message, cancellationToken: cancellationToken)
            .ToIOUnit();
    }
}