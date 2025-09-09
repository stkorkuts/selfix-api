using System.Text.Json.Serialization;

namespace Selfix.Infrastructure.Telegram.Callbacks;

internal sealed class ShowProductsCallbackData : CallbackData
{
    [JsonPropertyName("i")] public required bool IsFirstPayment { get; set; }
}