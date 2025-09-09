namespace Selfix.Shared.Settings;

public sealed class DatabaseSettings
{
    public const string KEY = "Database";
    public required string ConnectionString { get; init; }
}