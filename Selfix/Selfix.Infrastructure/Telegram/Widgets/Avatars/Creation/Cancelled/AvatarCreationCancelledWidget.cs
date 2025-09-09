using LanguageExt;
using Selfix.Shared.Extensions;
using Telegram.Bot;

namespace Selfix.Infrastructure.Telegram.Widgets.Avatars.Creation.Cancelled;

internal sealed class AvatarCreationCancelledWidget : IWidget<AvatarCreationCancelledWidgetContext>
{
    private readonly ITelegramBotClient _client;

    public AvatarCreationCancelledWidget(ITelegramBotClient client)
    {
        _client = client;
    }

    public IO<Unit> Show(AvatarCreationCancelledWidgetContext context, CancellationToken cancellationToken)
    {
        return
            from _1 in _client.SendMessage(context.Profile.Id,
                "Создание аватара отменено", cancellationToken: cancellationToken).ToIOUnit()
            select Unit.Default;
    }
}