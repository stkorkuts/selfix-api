using LanguageExt;
using Microsoft.Extensions.Options;
using Selfix.Shared.Constants;
using Selfix.Shared.Extensions;
using Selfix.Shared.Settings;
using Telegram.Bot;

namespace Selfix.Infrastructure.Telegram.Widgets.Onboarding.CustomPromptTutorial;

internal sealed class CustomPromptTutorialWidget : IWidget<CustomPromptTutorialWidgetContext>
{
    private readonly ITelegramBotClient _client;
    private readonly AssetsSettings _assetsSettings;

    public CustomPromptTutorialWidget(ITelegramBotClient client, IOptions<AssetsSettings> assetsOptions)
    {
        _client = client;
        _assetsSettings = assetsOptions.Value;
    }

    public IO<Unit> Show(CustomPromptTutorialWidgetContext context, CancellationToken cancellationToken)
    {
        return _client.SendPhoto(context.Profile.Id, _assetsSettings.Images.PromptImageUrl, "",
            cancellationToken: cancellationToken).ToIOUnit();
    }
}