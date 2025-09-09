using Selfix.Domain.Entities.Promocodes;

namespace Selfix.Application.Dtos.Promocodes;

internal static class PromocodeMappingExtensions
{
    public static PromocodeDto ToDto(this Promocode promocode)
    {
        return new PromocodeDto(promocode.Id, promocode.Code);
    }
}