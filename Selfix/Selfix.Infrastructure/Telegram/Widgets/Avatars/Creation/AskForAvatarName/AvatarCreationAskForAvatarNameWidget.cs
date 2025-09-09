using LanguageExt;
using Selfix.Infrastructure.Telegram.Callbacks;
using Selfix.Shared.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Selfix.Infrastructure.Telegram.Widgets.Avatars.Creation.AskForAvatarName;

internal sealed class AvatarCreationAskForAvatarNameWidget : IWidget<WidgetContext>
{
    private readonly ITelegramBotClient _client;

    public AvatarCreationAskForAvatarNameWidget(ITelegramBotClient client)
    {
        _client = client;
    }

    public IO<Unit> Show(WidgetContext context, CancellationToken cancellationToken)
    {
        return
            from callbackData in CallbackSerializer.Serialize(new CancelAvatarCreationCallbackData()).ToIO()
            let markup =
                new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("Отменить создание", callbackData))
            from _1 in _client.SendMessage(context.Profile.Id,
                "Введите имя для вашего нового аватара или отмените если нажали случайно",
                replyMarkup: markup, cancellationToken: cancellationToken).ToIOUnit()
            select Unit.Default;
    }
}