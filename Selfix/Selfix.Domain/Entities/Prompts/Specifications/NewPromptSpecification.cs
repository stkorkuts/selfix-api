using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Prompts;

namespace Selfix.Domain.Entities.Prompts.Specifications;

public sealed record NewPromptSpecification(
    PromptName Name,
    NaturalNumber NumberInOrder,
    PromptText Text);