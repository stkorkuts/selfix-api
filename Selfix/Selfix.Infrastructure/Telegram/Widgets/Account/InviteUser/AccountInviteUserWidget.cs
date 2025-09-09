using LanguageExt;
using Microsoft.Extensions.Options;
using Selfix.Shared.Extensions;
using Selfix.Shared.Settings;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Selfix.Infrastructure.Telegram.Widgets.Account.InviteUser;

internal sealed class AccountInviteUserWidget : IWidget<WidgetContext>
{
    private readonly ITelegramBotClient _client;
    private readonly TelegramBotSettings _settings;

    public AccountInviteUserWidget(ITelegramBotClient client, IOptions<TelegramBotSettings> options)
    {
        _client = client;
        _settings = options.Value;
    }

    public IO<Unit> Show(WidgetContext context, CancellationToken cancellationToken)
    {
        var refUrl = $"{_settings.TelegramBotUrl}?start={context.Profile.Id}";
        return
            from _1 in _client.SendMessage(context.Profile.Id,
                    "Пригласите друга в Selfix по реферальной ссылке и после первой оплаты получите по 10 генераций!",
                    replyMarkup: new InlineKeyboardMarkup(
                        InlineKeyboardButton.WithCopyText("Скопировать ссылку", refUrl)),
                    cancellationToken: cancellationToken)
                .ToIOUnit()
            select Unit.Default;
    }
}