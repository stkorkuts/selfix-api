using LanguageExt;
using Selfix.Shared.Extensions;
using Telegram.Bot;

namespace Selfix.Infrastructure.Telegram.Widgets.Prompts.Invalid;

internal sealed class CustomPromptInvalidWidget : IWidget<WidgetContext>
{
    private readonly ITelegramBotClient _client;

    public CustomPromptInvalidWidget(ITelegramBotClient client)
    {
        _client = client;
    }

    public IO<Unit> Show(WidgetContext context, CancellationToken cancellationToken)
    {
        return _client.SendMessage(context.Profile.Id,
            "Вы ввели не подходящий текст, он не должен быть слишком коротким или длинным, попробуйте, пожалуйста, снова",
            cancellationToken: cancellationToken).ToIOUnit();
    }
}