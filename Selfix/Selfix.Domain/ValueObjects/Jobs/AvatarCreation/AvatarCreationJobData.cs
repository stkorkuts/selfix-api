using LanguageExt;

namespace Selfix.Domain.ValueObjects.Jobs.AvatarCreation;

public sealed record AvatarCreationJobData(AvatarCreationJobInput Input, Option<AvatarCreationJobOutput> Output)
    : JobData;