using LanguageExt;

namespace Selfix.Domain.ValueObjects.Jobs.PromptProcessing;

public sealed record PromptProcessingJobData(PromptProcessingJobInput Input, Option<PromptProcessingJobOutput> Output)
    : JobData;