namespace Selfix.Infrastructure.Database.JsonDocumentSchema.Jobs;

internal sealed record PromptProcessingJobInputJsonDocumentSchema(
    string AvatarDescription,
    string RawPrompt,
    uint Quantity,
    long Seed
    );