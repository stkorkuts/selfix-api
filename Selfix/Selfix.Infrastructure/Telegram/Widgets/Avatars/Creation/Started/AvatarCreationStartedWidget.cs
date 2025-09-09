using LanguageExt;
using Selfix.Shared.Extensions;
using Telegram.Bot;

namespace Selfix.Infrastructure.Telegram.Widgets.Avatars.Creation.Started;

internal sealed class AvatarCreationStartedWidget : IWidget<AvatarCreationStartedWidgetContext>
{
    private readonly ITelegramBotClient _client;

    public AvatarCreationStartedWidget(ITelegramBotClient client)
    {
        _client = client;
    }

    public IO<Unit> Show(AvatarCreationStartedWidgetContext context, CancellationToken cancellationToken)
    {
        return
            from _1 in _client.SendMessage(context.Profile.Id,
                "Начато создание аватара! Обычно это занимает от 10 до 30 минут.\n" +
                "Если у вас уже есть аватар вы можете продолжать им пользоваться и создавать нереальные изображения пока создаётся новый!", cancellationToken: cancellationToken).ToIOUnit()
            select Unit.Default;
    }
}