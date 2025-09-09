using System.Text.Json.Serialization;

namespace Selfix.Infrastructure.Telegram.Payments.Schema;

internal sealed class ProviderDataReceipt
{
    [JsonPropertyName("items")] public required IEnumerable<ProviderDataReceiptItem> Items { get; set; }
};