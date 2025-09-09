namespace Selfix.Shared.Settings;

public sealed class TelegramBotPaymentsYooKassaSettings
{
    public const string KEY = "YooKassa";
    public required string PaymentProviderToken { get; set; }
}