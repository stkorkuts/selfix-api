using Selfix.Domain.ValueObjects.Quotas;

namespace Selfix.Application.Dtos.Quotas;

internal static class UserQuotasMappingExtensions
{
    public static UserQuotasDto ToDto(this UserQuotas quotas)
    {
        return new UserQuotasDto(quotas.CanGenerateAvatars(), quotas.CanGenerateImages());
    }
}