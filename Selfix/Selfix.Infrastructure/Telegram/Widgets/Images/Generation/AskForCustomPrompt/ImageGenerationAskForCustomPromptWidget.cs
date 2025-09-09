using LanguageExt;
using Selfix.Infrastructure.Telegram.Callbacks;
using Selfix.Shared.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Selfix.Infrastructure.Telegram.Widgets.Images.Generation.AskForCustomPrompt;

internal sealed class ImageGenerationAskForCustomPromptWidget : IWidget<WidgetContext>
{
    private readonly ITelegramBotClient _client;

    public ImageGenerationAskForCustomPromptWidget(ITelegramBotClient client)
    {
        _client = client;
    }

    public IO<Unit> Show(WidgetContext context, CancellationToken cancellationToken)
    {
        return
            from callback in CallbackSerializer.Serialize(new CustomPromptTutorialCallbackData()).ToIO()
            let markup = new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("Инструкция", callback))
            from _1 in _client.SendMessage(context.Profile.Id,
                "Опишите детально что бы вы хотели сгенерировать",
                replyMarkup: markup,
                cancellationToken: cancellationToken).ToIOUnit()
            select Unit.Default;
    }
}