using LanguageExt;
using Selfix.Shared.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Selfix.Infrastructure.Telegram.Widgets.Help;

internal sealed class HelpWidget : IWidget<HelpWidgetContext>
{
    private readonly ITelegramBotClient _client;

    public HelpWidget(ITelegramBotClient client)
    {
        _client = client;
    }

    public IO<Unit> Show(HelpWidgetContext context, CancellationToken cancellationToken)
    {
        var captions = "Для получения помощи, перейдите пожалуйста в нашу поддержку";
        var replyMarkup =
            new InlineKeyboardMarkup(InlineKeyboardButton.WithUrl("Перейти в поддержку",
                context.HelpCenterUrl.ToString()));
        return
            from _1 in _client.SendMessage(context.Profile.Id, captions, replyMarkup: replyMarkup,
                cancellationToken: cancellationToken).ToIOUnit()
            select Unit.Default;
    }
}