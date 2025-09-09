namespace Selfix.Shared.Settings;

public sealed class TelegramBotPaymentsSettings
{
    public const string KEY = "Payments";
    public required TelegramBotPaymentsYooKassaSettings YooKassa { get; init; }
}