using LanguageExt;
using Selfix.Application.Dtos;
using Selfix.Application.Dtos.Prompts;
using Selfix.Infrastructure.Telegram.Utils.ReplyMarkupExtensions;
using Selfix.Shared.Extensions;
using Selfix.Shared.Types;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Selfix.Infrastructure.Telegram.Widgets.Prompts.Page;

internal sealed class PromptsPageWidget : IWidget<PromptsPageWidgetContext>
{
    private readonly ITelegramBotClient _client;

    public PromptsPageWidget(ITelegramBotClient client)
    {
        _client = client;
    }

    public IO<Unit> Show(PromptsPageWidgetContext context, CancellationToken cancellationToken)
    {
        return context.PreviousPageMessage.Match(
            message => EditOld(context.Profile.Id, context.PromptsPage, message, cancellationToken),
            () => ShowNew(context.Profile.Id, context.PromptsPage, cancellationToken));
    }

    private IO<Unit> ShowNew(long profileId, Page<PromptDto> promptsPage, CancellationToken cancellationToken)
    {
        return
            from _1 in IO<Unit>.Pure(Unit.Default)
            let captions = "Выбери готовый образ и получи результат в течение одной минуты!"
            from keyboardMarkup in promptsPage.GetReplyMarkup().ToIO()
            from message in _client.SendMessage(profileId, captions,
                replyMarkup: keyboardMarkup,
                cancellationToken: cancellationToken).ToIO()
            select Unit.Default;
    }

    private IO<Unit> EditOld(long profileId, Page<PromptDto> promptsPage, Message message,
        CancellationToken cancellationToken)
    {
        return
            from replyMarkup in promptsPage.GetReplyMarkup().ToIO()
            from _1 in _client.EditMessageReplyMarkup(profileId, message.Id, replyMarkup,
                cancellationToken: cancellationToken).ToIOUnit()
            select Unit.Default;
    }
}