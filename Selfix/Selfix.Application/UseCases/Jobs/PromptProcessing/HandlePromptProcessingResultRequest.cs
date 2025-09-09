namespace Selfix.Application.UseCases.Jobs.PromptProcessing;

public sealed record HandlePromptProcessingResultRequest(Ulid JobId, PromptProcessingResultData Data);