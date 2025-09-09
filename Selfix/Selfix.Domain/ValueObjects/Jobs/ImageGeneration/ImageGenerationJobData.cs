using LanguageExt;

namespace Selfix.Domain.ValueObjects.Jobs.ImageGeneration;

public sealed record ImageGenerationJobData(ImageGenerationJobInput Input, Option<ImageGenerationJobOutput> Output)
    : JobData;