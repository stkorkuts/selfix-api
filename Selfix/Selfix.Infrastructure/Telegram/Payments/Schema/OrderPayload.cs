using System.Text.Json.Serialization;

namespace Selfix.Infrastructure.Telegram.Payments.Schema;

internal sealed class OrderPayload
{
    [JsonPropertyName("order_id")]
    public Ulid OrderId { get; set; }
}