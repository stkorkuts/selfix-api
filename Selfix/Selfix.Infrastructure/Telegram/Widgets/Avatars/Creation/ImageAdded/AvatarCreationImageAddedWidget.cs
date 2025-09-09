using LanguageExt;
using Selfix.Shared.Extensions;
using Telegram.Bot;

namespace Selfix.Infrastructure.Telegram.Widgets.Avatars.Creation.ImageAdded;

internal sealed class AvatarCreationImageAddedWidget : IWidget<AvatarCreationImageAddedWidgetContext>
{
    private readonly ITelegramBotClient _client;

    public AvatarCreationImageAddedWidget(ITelegramBotClient client)
    {
        _client = client;
    }

    public IO<Unit> Show(AvatarCreationImageAddedWidgetContext context, CancellationToken cancellationToken)
    {
        return _client.SendMessage(context.Profile.Id,
            $"Загружено изображений: {context.TotalImagesLoaded}", cancellationToken: cancellationToken).ToIOUnit();
    }
}