using LanguageExt;
using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Common;

namespace Selfix.Application.ServicesAbstractions.Notifications;

public interface INotificationsService
{
    IO<Unit> ScheduleSendTrialPackageToUserIfHasNoPayments(Id<User, Ulid> id, NaturalNumber tryNumber,
        CancellationToken cancellationToken);
}