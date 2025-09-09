namespace Selfix.Infrastructure.Telegram;

internal static class TelegramConstants
{
    public const string GENERATE_PREDEFINED_COMMAND_ID = "Образы";
    public const string GENERATE_ANY_COMMAND_ID = "Режим творца";
    public const string AVATARS_COMMAND_ID = "Аватары";
    public const string ACCOUNT_COMMAND_ID = "Профиль";
    public const string SETTINGS_COMMAND_ID = "Настройки";
    public const string HELP_COMMAND_ID = "Помощь";

    public const uint PUBLIC_PROMPTS_PAGE_SIZE = 8;

    public static readonly IReadOnlySet<string> PANEL_COMMANDS_SET = new HashSet<string>([
        GENERATE_ANY_COMMAND_ID,
        GENERATE_PREDEFINED_COMMAND_ID,
        AVATARS_COMMAND_ID,
        ACCOUNT_COMMAND_ID,
        SETTINGS_COMMAND_ID,
        HELP_COMMAND_ID
    ]);
}