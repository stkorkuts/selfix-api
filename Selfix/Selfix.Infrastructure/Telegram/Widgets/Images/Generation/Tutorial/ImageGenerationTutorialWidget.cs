using LanguageExt;
using Telegram.Bot;

namespace Selfix.Infrastructure.Telegram.Widgets.Images.Generation.Tutorial;

internal sealed class ImageGenerationTutorialWidget : IWidget<WidgetContext>
{
    private readonly ITelegramBotClient _client;

    public ImageGenerationTutorialWidget(ITelegramBotClient client)
    {
        _client = client;
    }

    public IO<Unit> Show(WidgetContext context, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}