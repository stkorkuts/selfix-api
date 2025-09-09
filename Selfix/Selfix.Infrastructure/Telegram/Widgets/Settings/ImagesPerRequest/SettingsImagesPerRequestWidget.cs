using LanguageExt;
using Selfix.Infrastructure.Telegram.Callbacks;
using Selfix.Shared.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Selfix.Infrastructure.Telegram.Widgets.Settings.ImagesPerRequest;

internal sealed class SettingsImagesPerRequestWidget : IWidget<WidgetContext>
{
    private readonly ITelegramBotClient _client;

    public SettingsImagesPerRequestWidget(ITelegramBotClient client)
    {
        _client = client;
    }

    public IO<Unit> Show(WidgetContext context, CancellationToken cancellationToken)
    {
        return
            from markup in GetMarkup().ToIO()
            from _1 in _client.SendMessage(
                context.Profile.Id,
                "Выберите количество генерируемых фотографий за раз. Увеличение количества одновременных фотографий немного увеличивает время генерации. Количество фотографий за раз:",
                replyMarkup: markup,
                cancellationToken: cancellationToken).ToIOUnit()
            select Unit.Default;
    }

    private static Fin<InlineKeyboardMarkup> GetMarkup()
    {
        return
            from oneCallback in CallbackSerializer.Serialize(
                new SettingsSetImagesPerRequestCallbackData
                    { ImagesPerRequest = 1 })
            from twoCallback in CallbackSerializer.Serialize(
                new SettingsSetImagesPerRequestCallbackData
                    { ImagesPerRequest = 2 })
            from threeCallback in CallbackSerializer.Serialize(
                new SettingsSetImagesPerRequestCallbackData
                    { ImagesPerRequest = 3 })
            select new InlineKeyboardMarkup(
                InlineKeyboardButton.WithCallbackData("1", oneCallback),
                InlineKeyboardButton.WithCallbackData("2", twoCallback),
                InlineKeyboardButton.WithCallbackData("3", threeCallback)
            );
    }
}