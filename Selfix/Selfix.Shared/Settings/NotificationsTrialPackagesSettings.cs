namespace Selfix.Shared.Settings;

public class NotificationsTrialPackagesSettings
{
    public const string KEY = "TrialPackages";
    public required bool ShowTrialPackageNotification { get; set; }
    public required int[] ShowInMinutes { get; set; }
}