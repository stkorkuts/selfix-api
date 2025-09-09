using Selfix.Domain.ValueObjects.Avatars;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Prompts;

namespace Selfix.Domain.ValueObjects.Jobs.PromptProcessing;

public sealed record PromptProcessingJobInput(AvatarDescription AvatarDescription, PromptText RawPrompt, NaturalNumber Quantity, long Seed);