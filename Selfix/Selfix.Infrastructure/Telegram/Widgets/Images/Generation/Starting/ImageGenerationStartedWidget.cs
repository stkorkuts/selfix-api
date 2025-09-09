using LanguageExt;
using Selfix.Application.Dtos;
using Selfix.Application.Dtos.Prompts;
using Selfix.Application.Dtos.TelegramProfiles;
using Selfix.Infrastructure.Telegram.Widgets.Prompts.Page;
using Selfix.Shared.Extensions;
using Selfix.Shared.Types;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Selfix.Infrastructure.Telegram.Widgets.Images.Generation.Starting;

internal sealed class
    ImageGenerationStartedWidget : IWidget<ImageGenerationStartedWidgetContext>
{
    private readonly ITelegramBotClient _client;

    public ImageGenerationStartedWidget(ITelegramBotClient client)
    {
        _client = client;
    }

    public IO<Unit> Show(ImageGenerationStartedWidgetContext context,
        CancellationToken cancellationToken)
    {
        return context.PredefinedPromptData.Match(
            meta => ShowGenerationFromPredefinedPromptStarted(meta.Item1, context.Profile, meta.Item2,
                cancellationToken),
            () => ShowGenerationFromCustomPromptStarted(context.Profile, cancellationToken));
    }

    private IO<Unit> ShowGenerationFromPredefinedPromptStarted(
        Message message,
        TelegramProfileDto profile,
        Page<PromptDto> promptsPage,
        CancellationToken cancellationToken)
    {
        return
            from _1 in _client.DeleteMessage(profile.Id, message.Id, cancellationToken)
                .ToIO()
            from _2 in new PromptsPageWidget(_client).Show(
                new PromptsPageWidgetContext(profile, promptsPage, Option<Message>.None), cancellationToken)
            from _3 in _client.SendMessage(profile.Id, "Генерация изображения началась (30-90 секунд)...",
                cancellationToken: cancellationToken).ToIOUnit()
            select Unit.Default;
    }

    private IO<Unit> ShowGenerationFromCustomPromptStarted(TelegramProfileDto profile,
        CancellationToken cancellationToken)
    {
        return _client.SendMessage(profile.Id, "Генерация изображения началась (30-90 секунд)...",
            cancellationToken: cancellationToken).ToIOUnit();
    }
}