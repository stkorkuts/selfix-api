using LanguageExt;
using Selfix.Application.Dtos;
using Selfix.Application.Dtos.Prompts;
using Selfix.Application.Dtos.TelegramProfiles;
using Selfix.Shared.Types;
using Telegram.Bot.Types;

namespace Selfix.Infrastructure.Telegram.Widgets.Images.Generation.Starting;

internal sealed record ImageGenerationStartedWidgetContext(
    TelegramProfileDto Profile,
    Option<(Message, Page<PromptDto>)> PredefinedPromptData) : WidgetContext(Profile);