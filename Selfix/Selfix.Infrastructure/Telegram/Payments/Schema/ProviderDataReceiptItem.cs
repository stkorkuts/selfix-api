using System.Text.Json.Serialization;

namespace Selfix.Infrastructure.Telegram.Payments.Schema;

internal sealed class ProviderDataReceiptItem
{
    [JsonPropertyName("description")] public required string Description { get; set; }
    [JsonPropertyName("quantity")] public required int Quantity { get; set; }
    [JsonPropertyName("amount")] public required ProviderDataReceiptItemAmount Amount { get; set; }
    [JsonPropertyName("vat_code")] public required int VatCode { get; set; }
    [JsonPropertyName("payment_mode")] public required string PaymentMode { get; set; }
    [JsonPropertyName("payment_subject")] public required string PaymentSubject { get; set; }
};