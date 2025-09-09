namespace Selfix.Application.UseCases.Telegram.Prompts.Get;

public sealed record GetPromptsRequest(uint PageSize, uint PageIndex);