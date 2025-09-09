using System.Text;
using LanguageExt;
using Microsoft.Extensions.Options;
using Selfix.Domain.ValueObjects.Products.Packages;
using Selfix.Infrastructure.Telegram.Utils.ReplyMarkupExtensions;
using Selfix.Shared.Constants;
using Selfix.Shared.Extensions;
using Selfix.Shared.Settings;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Selfix.Infrastructure.Telegram.Widgets.Products.List;

internal sealed class ProductsListWidget : IWidget<ProductsListWidgetContext>
{
    private readonly ITelegramBotClient _client;
    private readonly AssetsSettings _assetsSettings;

    public ProductsListWidget(ITelegramBotClient client, IOptions<AssetsSettings> assetsOptions)
    {
        _client = client;
        _assetsSettings = assetsOptions.Value;
    }

    public IO<Unit> Show(ProductsListWidgetContext context, CancellationToken cancellationToken)
    {
        return context.User.HasPayments switch
        {
            true => ShowRegularProposal(context, cancellationToken),
            false => ShowFirstProposal(context, cancellationToken)
        };
    }

    private IO<Unit> ShowRegularProposal(ProductsListWidgetContext context, CancellationToken cancellationToken)
    {
        var caption = new StringBuilder("Пакеты:\n");
        context.Products
            .Map(x =>
            {
                var priceWithoutDiscount = x.Price + x.Discount;
                return $"{x.Name} - {x.Price}р. (<s>{priceWithoutDiscount}р.</s>)";
            })
            .Iter(x => caption.AppendLine(x));

        return
            from markup in context.Products.GenerateMarkup(context.IsPromocode).ToIO()
            from _1 in _client.SendMessage(context.Profile.Id, caption.ToString(), replyMarkup: markup,
                parseMode: ParseMode.Html, cancellationToken: cancellationToken).ToIOUnit()
            select Unit.Default;
    }

    private IO<Unit> ShowFirstProposal(ProductsListWidgetContext context, CancellationToken cancellationToken)
    {
        var caption = new StringBuilder("Выгодный пакет с большой скидкой:\n");
        context.Products
            .Map(x =>
            {
                var priceWithoutDiscount = x.Price + x.Discount;
                return $"{x.Name} - {x.Price}р. (<s>{priceWithoutDiscount}р.</s>)";
            })
            .Iter(x => caption.AppendLine(x));

        return
            from markup in context.Products.GenerateMarkup(context.IsPromocode).ToIO()
            from _1 in _client.SendPhoto(context.Profile.Id, _assetsSettings.Images.Price1ImageUrl,
                caption.ToString(), replyMarkup: markup, parseMode: ParseMode.Html,
                cancellationToken: cancellationToken).ToIOUnit()
            select Unit.Default;
    }
}