namespace Selfix.Application.UseCases.Jobs.PromptProcessing;

public sealed record FailedPromptProcessingResultData(string Error) : PromptProcessingResultData;