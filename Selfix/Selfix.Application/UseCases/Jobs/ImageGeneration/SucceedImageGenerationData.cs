using LanguageExt;

namespace Selfix.Application.UseCases.Jobs.ImageGeneration;

public sealed record SucceedImageGenerationData(string[] Paths) : ImageGenerationResultData;