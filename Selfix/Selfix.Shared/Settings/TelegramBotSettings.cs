namespace Selfix.Shared.Settings;

public sealed class TelegramBotSettings
{
    public const string KEY = "TelegramBot";
    public required string Token { get; init; }
    public required string TelegramBotUrl { get; set; }
    public required string HelpAccountUrl { get; init; }
    public required TelegramBotPaymentsSettings Payments { get; init; }
}