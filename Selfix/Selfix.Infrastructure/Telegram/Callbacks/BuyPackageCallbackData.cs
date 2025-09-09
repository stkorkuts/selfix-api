using System.Text.Json.Serialization;

namespace Selfix.Infrastructure.Telegram.Callbacks;

internal sealed class BuyPackageCallbackData : CallbackData
{
    [JsonPropertyName("p")] public required Ulid PackageId { get; init; }
    [JsonPropertyName("i")] public required bool IsPromocode { get; init; }
}