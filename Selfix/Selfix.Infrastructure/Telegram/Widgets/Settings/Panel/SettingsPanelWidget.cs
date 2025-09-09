using System.Collections.Immutable;
using LanguageExt;
using Selfix.Infrastructure.Telegram.Callbacks;
using Selfix.Shared;
using Selfix.Shared.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Selfix.Infrastructure.Telegram.Widgets.Settings.Panel;

internal sealed class SettingsPanelWidget : IWidget<WidgetContext>
{
    private static readonly ImmutableDictionary<ImageAspectRatioEnum, string> ASPECT_RATIO_TO_NAME =
        ImmutableDictionary.CreateRange(new Dictionary<ImageAspectRatioEnum, string>
        {
            { ImageAspectRatioEnum.Square1X1, "Квадрат (1:1)" },
            { ImageAspectRatioEnum.Portrait9X16, "Портрет (9x16)" },
            { ImageAspectRatioEnum.Landscape16X9, "Ландшафт (16x9)" }
        });

    private readonly ITelegramBotClient _client;

    public SettingsPanelWidget(ITelegramBotClient client)
    {
        _client = client;
    }

    public IO<Unit> Show(WidgetContext context, CancellationToken cancellationToken)
    {
        var captions = @$"
Здесь ты можешь изменить настройки Selfix.

Текущий формат генерации фото: {ASPECT_RATIO_TO_NAME[context.Profile.Settings.ImageAspectRatio]}
Текущее количество одновременно генерируемых изображений: {context.Profile.Settings.ImagesToGeneratePerRequest}";
        return
            from replyMarkup in GetReplyMarkup().ToIO()
            from _1 in _client.SendMessage(context.Profile.Id, captions, replyMarkup: replyMarkup,
                cancellationToken: cancellationToken).ToIOUnit()
            select Unit.Default;
    }

    private static Fin<InlineKeyboardMarkup> GetReplyMarkup()
    {
        return
            from changeAspectRatioCallbackData in CallbackSerializer.Serialize(
                new SettingsShowAspectRatioOptionsCallbackData())
            from changeImagesPerRequestCallbackData in CallbackSerializer.Serialize(
                new SettingsShowImagesPerRequestOptionsCallbackData())
            select new InlineKeyboardMarkup()
                .AddNewRow(InlineKeyboardButton.WithCallbackData("Соотношение сторон",
                    changeAspectRatioCallbackData))
                .AddNewRow(InlineKeyboardButton.WithCallbackData("Количество изображений за раз",
                    changeImagesPerRequestCallbackData));
    }
}