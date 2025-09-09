namespace Selfix.Shared.Settings;

public sealed class NotificationsSettings
{
    public const string KEY = "Notifications";
    public required string DbConnectionString { get; init; }
    public required NotificationsTrialPackagesSettings TrialPackages { get; set; }
}