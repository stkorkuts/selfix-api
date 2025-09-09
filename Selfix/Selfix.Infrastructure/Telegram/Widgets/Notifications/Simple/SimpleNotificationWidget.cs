using LanguageExt;
using Selfix.Shared.Extensions;
using Telegram.Bot;

namespace Selfix.Infrastructure.Telegram.Widgets.Notifications.Simple;

internal sealed class SimpleNotificationWidget : IWidget<SimpleNotificationWidgetContext>
{
    private readonly ITelegramBotClient _client;

    public SimpleNotificationWidget(ITelegramBotClient client)
    {
        _client = client;
    }

    public IO<Unit> Show(SimpleNotificationWidgetContext context, CancellationToken cancellationToken)
    {
        return _client.SendMessage(context.Profile.Id, context.Text, cancellationToken: cancellationToken).ToIOUnit();
    }
}