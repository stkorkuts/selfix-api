using System.Globalization;
using Selfix.Application.UseCases.Notifications.ShowTrialPackage;

namespace Selfix.Infrastructure.Notifications;

public class NotificationsExecutor
{
    private readonly IShowTrialPackageNotificationUseCase _showTrialPackageNotificationUseCase;

    public NotificationsExecutor(IShowTrialPackageNotificationUseCase showTrialPackageNotificationUseCase)
    {
        _showTrialPackageNotificationUseCase = showTrialPackageNotificationUseCase;
    }

    public Task SendTrialPackageToUserIfHasNoPayments(string userId, int currentTryNumber)
    {
        var userUlid = Ulid.Parse(userId, CultureInfo.InvariantCulture);
        return _showTrialPackageNotificationUseCase
            .Execute(new ShowTrialPackageNotificationRequest(userUlid, (uint)currentTryNumber), CancellationToken.None)
            .RunAsync()
            .AsTask();
    }
}