using LanguageExt;
using Selfix.Domain.ValueObjects.Promocodes;

namespace Selfix.Application.UseCases.Telegram.Promocodes.ValidatePromocode;

internal sealed class ValidatePromocodeUseCase : IValidatePromocodeUseCase
{
    public IO<ValidatePromocodeResponse> Execute(ValidatePromocodeRequest request,
        CancellationToken cancellationToken)
    {
        return IO<ValidatePromocodeResponse>.Pure(new ValidatePromocodeResponse(
            AlphanumericString.From(request.Promocode).Match(_ => true, _ => false)
        ));
    }
}