using Selfix.Domain.Entities.Prompts;

namespace Selfix.Application.Dtos.Prompts;

internal static class PromptMappingExtensions
{
    public static PromptDto ToDto(this Prompt prompt)
    {
        return new PromptDto(prompt.Id, prompt.Name);
    }
}