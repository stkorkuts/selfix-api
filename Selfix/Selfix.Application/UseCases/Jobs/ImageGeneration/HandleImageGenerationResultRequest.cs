namespace Selfix.Application.UseCases.Jobs.ImageGeneration;

public sealed record HandleImageGenerationResultRequest(Ulid JobId, ImageGenerationResultData GenerationResultData);