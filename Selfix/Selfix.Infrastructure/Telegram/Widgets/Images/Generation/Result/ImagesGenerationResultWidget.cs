using LanguageExt;
using Microsoft.Extensions.Options;
using Selfix.Shared.Extensions;
using Selfix.Shared.Settings;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Selfix.Infrastructure.Telegram.Widgets.Images.Result;

internal sealed class ImagesGenerationResultWidget : IWidget<ImagesGenerationResultWidgetContext>
{
    private readonly ITelegramBotClient _client;
    private readonly TelegramBotSettings _settings;

    public ImagesGenerationResultWidget(ITelegramBotClient client, IOptions<TelegramBotSettings> options)
    {
        _client = client;
        _settings = options.Value;
    }

    public IO<Unit> Show(ImagesGenerationResultWidgetContext context, CancellationToken cancellationToken)
    {
        var link = $"{_settings.TelegramBotUrl}?start={context.Profile.Id}";
        var caption = $"<a href=\"{link}\">Сделано в Selfix AI</a>";
        return
            context.ImagesWithSignedUris.Count() switch
            {
                > 1 and <= 10 => SendAlbumWithCaption(context, caption, cancellationToken),
                1 => _client.SendPhoto(context.Profile.Id, context.ImagesWithSignedUris.First().SignedUri.ToString(),
                    caption, ParseMode.Html, cancellationToken: cancellationToken).ToIOUnit(),
                _ => throw new Exception(
                    "There should be no case when image generation result does not contain any images or contains more than 10 images")
            };
    }

    private IO<Unit> SendAlbumWithCaption(ImagesGenerationResultWidgetContext context, string caption,
        CancellationToken cancellationToken)
    {
        var mediaPhotos = context.ImagesWithSignedUris.Map(item =>
            new InputMediaPhoto(InputFile.FromUri(item.SignedUri.ToString()))
            {
                Caption = caption,
                ParseMode = ParseMode.Html
            });
        return _client.SendMediaGroup(context.Profile.Id, mediaPhotos.AsEnumerable(), cancellationToken: cancellationToken).ToIOUnit();
    }
}