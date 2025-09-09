using LanguageExt;
using Selfix.Infrastructure.Telegram.Callbacks;
using Selfix.Shared.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Selfix.Infrastructure.Telegram.Widgets.Account.Info;

internal sealed class AccountInfoWidget : IWidget<AccountInfoWidgetContext>
{
    private readonly ITelegramBotClient _client;

    public AccountInfoWidget(ITelegramBotClient client)
    {
        _client = client;
    }

    public IO<Unit> Show(AccountInfoWidgetContext context, CancellationToken cancellationToken)
    {
        var captions = $"Информация о профиле:\n" +
                           $"Остаток генераций фото: {context.User.AvailableImageGenerations}\n" +
                           $"Доступно генераций аватаров: {context.User.AvailableAvatarGenerations}\n" +
                           $"Идентификатор пользователя: {context.User.Id}\n" +
                           $"Идентификатор телеграм: {context.Profile.Id}";
        return
            from replyMarkup in GetReplyMarkup(context.User.HasPayments).ToIO()
            from _1 in _client.SendMessage(context.Profile.Id, captions, replyMarkup: replyMarkup,
                cancellationToken: cancellationToken).ToIOUnit()
            select Unit.Default;
    }

    private static Fin<InlineKeyboardMarkup> GetReplyMarkup(bool hasPayments)
    {
        return
            from showProductsCallbackData in CallbackSerializer.Serialize(new ShowProductsCallbackData
                { IsFirstPayment = !hasPayments })
            from inviteUserCallbackData in CallbackSerializer.Serialize(new InviteUserCallbackData())
            from buyPromocodeCallbackData in CallbackSerializer.Serialize(new BuyPromocodeCallbackData())
            select new InlineKeyboardMarkup()
                .AddNewRow(InlineKeyboardButton.WithCallbackData("Купить генерации", showProductsCallbackData))
                .AddNewRow(InlineKeyboardButton.WithCallbackData("Поделиться и получить бонус", inviteUserCallbackData))
                .AddNewRow(InlineKeyboardButton.WithCallbackData("Подарить Selfix", buyPromocodeCallbackData));
    }
}