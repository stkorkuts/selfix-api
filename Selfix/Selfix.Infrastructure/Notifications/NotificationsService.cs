using Hangfire;
using LanguageExt;
using Microsoft.Extensions.Options;
using Selfix.Application.ServicesAbstractions.Notifications;
using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Shared.Settings;

namespace Selfix.Infrastructure.Notifications;

internal sealed class NotificationsService : INotificationsService
{
    private readonly NotificationsSettings _notificationsSettings;

    public NotificationsService(IOptions<NotificationsSettings> notificationsOptions)
    {
        _notificationsSettings = notificationsOptions.Value;
    }
    
    public IO<Unit> ScheduleSendTrialPackageToUserIfHasNoPayments(Id<User, Ulid> id, NaturalNumber tryNumber, CancellationToken cancellationToken)
    {
        if(_notificationsSettings.TrialPackages.ShowInMinutes.Length < tryNumber) return IO<Unit>.Pure(Unit.Default);
        var executeIn = TimeSpan.FromMinutes(_notificationsSettings.TrialPackages.ShowInMinutes[tryNumber - 1]);
        return IO<Unit>.Lift(() =>
        {
            var idStr = (string)id;
            var tryNumberInt = (int)tryNumber;
            BackgroundJob.Schedule<NotificationsExecutor>(service => service.SendTrialPackageToUserIfHasNoPayments(idStr, tryNumberInt), executeIn);
            return Unit.Default;
        });
    }
}