using LanguageExt;
using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Statistics;

namespace Selfix.Application.ServicesAbstractions.Statistics;

public interface IStatisticsService
{
    IO<UserStatistics> GetUserStatistics(Id<User, Ulid> userId, CancellationToken cancellationToken);
    IO<ApplicationStatistics> GetApplicationStatistics(CancellationToken cancellationToken);
}