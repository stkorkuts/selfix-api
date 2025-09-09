namespace Selfix.Application.UseCases.Notifications.ShowTrialPackage;

public sealed record ShowTrialPackageNotificationRequest(Ulid UserId, uint CurrentTryNumber);