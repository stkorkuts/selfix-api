using Selfix.Shared;

namespace Selfix.Infrastructure.Database.JsonDocumentSchema.Jobs;

internal sealed record ImageGenerationJobInputJsonDocumentSchema(
    string AvatarLoraPath,
    string Prompt,
    uint Quantity,
    long Seed,
    ImageAspectRatioEnum ImageAspectRatio
    );