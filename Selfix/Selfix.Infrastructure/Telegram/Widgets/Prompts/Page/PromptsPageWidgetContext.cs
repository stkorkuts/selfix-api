using LanguageExt;
using Selfix.Application.Dtos;
using Selfix.Application.Dtos.Prompts;
using Selfix.Application.Dtos.TelegramProfiles;
using Selfix.Shared.Types;
using Telegram.Bot.Types;

namespace Selfix.Infrastructure.Telegram.Widgets.Prompts.Page;

internal sealed record PromptsPageWidgetContext(
    TelegramProfileDto Profile,
    Page<PromptDto> PromptsPage,
    Option<Message> PreviousPageMessage)
    : WidgetContext(Profile);