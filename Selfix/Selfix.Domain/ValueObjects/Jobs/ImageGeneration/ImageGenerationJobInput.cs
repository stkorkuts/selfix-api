using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.ObjectStorage;
using Selfix.Domain.ValueObjects.Prompts;
using Selfix.Shared;

namespace Selfix.Domain.ValueObjects.Jobs.ImageGeneration;

public sealed record ImageGenerationJobInput(
    OSFilePath AvatarLoraPath,
    PromptText Prompt,
    NaturalNumber Quantity,
    long Seed,
    ImageAspectRatioEnum AspectRatio);