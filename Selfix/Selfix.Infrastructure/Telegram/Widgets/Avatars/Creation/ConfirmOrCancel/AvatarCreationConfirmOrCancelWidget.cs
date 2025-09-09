using LanguageExt;
using Selfix.Infrastructure.Telegram.Callbacks;
using Selfix.Shared.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Selfix.Infrastructure.Telegram.Widgets.Avatars.Creation.ConfirmOrCancel;

internal sealed class AvatarCreationConfirmOrCancelWidget : IWidget<AvatarCreationConfirmOrCancelWidgetContext>
{
    private readonly ITelegramBotClient _client;

    public AvatarCreationConfirmOrCancelWidget(ITelegramBotClient client)
    {
        _client = client;
    }

    public IO<Unit> Show(AvatarCreationConfirmOrCancelWidgetContext context, CancellationToken cancellationToken)
    {
        return
            from markup in GetMarkup().ToIO()
            from _1 in _client.SendMessage(context.Profile.Id,
                "Изображения загружены! Подтверждаете создание аватара? Обратите внимание что после подтверждения аватар нельзя будет изменить",
                replyMarkup: markup, cancellationToken: cancellationToken).ToIOUnit()
            select Unit.Default;
    }

    private static Fin<InlineKeyboardMarkup> GetMarkup()
    {
        return
            from cancelCallbackData in CallbackSerializer.Serialize(new CancelAvatarCreationCallbackData())
            from confirmCallbackData in CallbackSerializer.Serialize(new ConfirmAvatarCreationCallbackData())
            select new InlineKeyboardMarkup([
                [InlineKeyboardButton.WithCallbackData("Отменить", cancelCallbackData)],
                [InlineKeyboardButton.WithCallbackData("Подтвердить", confirmCallbackData)]
            ]);
    }
}