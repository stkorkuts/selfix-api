namespace Selfix.Application.UseCases.Telegram.Prompts.ProcessRawUserPrompt;

public sealed record ProcessRawUserPromptRequest(long TelegramProfileId, string Prompt);