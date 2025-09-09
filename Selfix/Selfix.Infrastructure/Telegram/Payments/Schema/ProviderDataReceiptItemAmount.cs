using System.Text.Json.Serialization;

namespace Selfix.Infrastructure.Telegram.Payments.Schema;

internal sealed class ProviderDataReceiptItemAmount
{
    [JsonPropertyName("value")] public required int Value { get; set; }
    [JsonPropertyName("currency")] public required string Currency { get; set; }
};