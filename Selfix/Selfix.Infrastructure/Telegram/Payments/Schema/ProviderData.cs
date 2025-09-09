using System.Text.Json.Serialization;

namespace Selfix.Infrastructure.Telegram.Payments.Schema;

internal sealed class ProviderData
{
    [JsonPropertyName("receipt")] public required ProviderDataReceipt Receipt { get; set; }
};