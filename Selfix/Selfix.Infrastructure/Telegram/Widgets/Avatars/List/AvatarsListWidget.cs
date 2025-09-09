using LanguageExt;
using Selfix.Infrastructure.Telegram.Utils.ReplyMarkupExtensions;
using Selfix.Shared.Extensions;
using Telegram.Bot;

namespace Selfix.Infrastructure.Telegram.Widgets.Avatars.List;

internal sealed class AvatarsListWidget : IWidget<AvatarsListWidgetContext>
{
    private readonly ITelegramBotClient _client;

    public AvatarsListWidget(ITelegramBotClient client)
    {
        _client = client;
    }

    public IO<Unit> Show(AvatarsListWidgetContext context, CancellationToken cancellationToken)
    {
        return
            from replyMarkup in context.Avatars.GetReplyMarkup().ToIO()
            let captions = "Выберите аватар или создайте новый"
            from _1 in _client.SendMessage(
                context.Profile.Id,
                captions,
                replyMarkup: replyMarkup,
                cancellationToken: cancellationToken).ToIOUnit()
            select Unit.Default;
    }
}