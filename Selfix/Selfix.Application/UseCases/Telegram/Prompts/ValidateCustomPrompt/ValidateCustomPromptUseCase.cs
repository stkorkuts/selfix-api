using LanguageExt;
using Selfix.Domain.ValueObjects.Prompts;

namespace Selfix.Application.UseCases.Telegram.Prompts.ValidateCustomPrompt;

internal sealed class ValidateCustomPromptUseCase : IValidateCustomPromptUseCase
{
    public IO<ValidateCustomPromptResponse> Execute(ValidateCustomPromptRequest request,
        CancellationToken cancellationToken)
    {
        var isValid = PromptText.From(request.Prompt).Match(_ => true, _ => false);
        return IO<ValidateCustomPromptResponse>.Pure(new ValidateCustomPromptResponse(isValid));
    }
}