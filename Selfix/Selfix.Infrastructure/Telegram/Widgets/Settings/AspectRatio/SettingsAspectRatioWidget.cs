using LanguageExt;
using Selfix.Infrastructure.Telegram.Callbacks;
using Selfix.Shared;
using Selfix.Shared.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Selfix.Infrastructure.Telegram.Widgets.Settings.AspectRatio;

internal sealed class SettingsAspectRatioWidget : IWidget<WidgetContext>
{
    private readonly ITelegramBotClient _client;

    public SettingsAspectRatioWidget(ITelegramBotClient client)
    {
        _client = client;
    }

    public IO<Unit> Show(WidgetContext context, CancellationToken cancellationToken)
    {
        return
            from markup in GetMarkup().ToIO()
            from _1 in _client.SendMessage(
                context.Profile.Id,
                "Выберите соотношение сторон для изображений:",
                replyMarkup: markup,
                cancellationToken: cancellationToken).ToIOUnit()
            select Unit.Default;
    }

    private static Fin<InlineKeyboardMarkup> GetMarkup()
    {
        return
            from squareCallback in CallbackSerializer.Serialize(
                new SettingsSetAspectRatioCallbackData
                    { NewRatio = ImageAspectRatioEnum.Square1X1 })
            from landscapeCallback in CallbackSerializer.Serialize(
                new SettingsSetAspectRatioCallbackData
                    { NewRatio = ImageAspectRatioEnum.Landscape16X9 })
            from portraitCallback in CallbackSerializer.Serialize(
                new SettingsSetAspectRatioCallbackData
                    { NewRatio = ImageAspectRatioEnum.Portrait9X16 })
            select new InlineKeyboardMarkup([
                [InlineKeyboardButton.WithCallbackData("Квадрат (1:1)", squareCallback)],
                [InlineKeyboardButton.WithCallbackData("Ландшафт (16:9)", landscapeCallback)],
                [InlineKeyboardButton.WithCallbackData("Портрет (9:16)", portraitCallback)]
            ]);
    }
}