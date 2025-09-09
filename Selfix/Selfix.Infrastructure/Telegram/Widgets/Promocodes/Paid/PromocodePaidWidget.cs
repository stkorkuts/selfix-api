using LanguageExt;
using Selfix.Shared.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Selfix.Infrastructure.Telegram.Widgets.Promocodes.Paid;

internal sealed class PromocodePaidWidget : IWidget<PromocodePaidWidgetContext>
{
    private readonly ITelegramBotClient _client;

    public PromocodePaidWidget(ITelegramBotClient client)
    {
        _client = client;
    }

    public IO<Unit> Show(PromocodePaidWidgetContext context, CancellationToken cancellationToken)
    {
        var markup = new InlineKeyboardMarkup(InlineKeyboardButton.WithCopyText("Копировать", context.Promocode.Code));
        return _client.SendMessage(context.Profile.Id, $"Промокод куплен! Вот код для активации:\n{context.Promocode.Code}", 
                replyMarkup: markup, cancellationToken: cancellationToken).ToIOUnit();
    }
}