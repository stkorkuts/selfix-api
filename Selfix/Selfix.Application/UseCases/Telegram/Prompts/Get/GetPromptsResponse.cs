using Selfix.Application.Dtos;
using Selfix.Application.Dtos.Prompts;
using Selfix.Shared.Types;

namespace Selfix.Application.UseCases.Telegram.Prompts.Get;

public sealed record GetPromptsResponse(Page<PromptDto> PromptsPage);