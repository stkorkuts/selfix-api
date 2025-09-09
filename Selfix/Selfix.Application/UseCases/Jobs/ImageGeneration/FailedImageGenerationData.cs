namespace Selfix.Application.UseCases.Jobs.ImageGeneration;

public sealed record FailedImageGenerationData(string Error) : ImageGenerationResultData;