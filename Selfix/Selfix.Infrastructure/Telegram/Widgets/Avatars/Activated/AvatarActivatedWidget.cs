using LanguageExt;
using Selfix.Shared.Extensions;
using Telegram.Bot;

namespace Selfix.Infrastructure.Telegram.Widgets.Avatars.Activated;

internal sealed class AvatarActivatedWidget : IWidget<AvatarActivatedWidgetContext>
{
    private readonly ITelegramBotClient _client;

    public AvatarActivatedWidget(ITelegramBotClient client)
    {
        _client = client;
    }

    public IO<Unit> Show(AvatarActivatedWidgetContext context, CancellationToken cancellationToken)
    {
        var caption = $"Выбран аватар: {context.Avatar.Name}.\n" +
                      $"Теперь он будет использоваться при генерации изображений.\n" +
                      $"Для генерации изображений перейдите в готовые образы или активируйте режим творца в меню панели";
        return _client.SendMessage(context.Profile.Id, caption, cancellationToken: cancellationToken).ToIOUnit();
    }
}