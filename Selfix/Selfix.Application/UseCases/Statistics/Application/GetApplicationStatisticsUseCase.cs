using System;
using LanguageExt;
using LanguageExt.Common;
using Selfix.Application.ServicesAbstractions.Database.Repositories;
using Selfix.Application.ServicesAbstractions.Database.Repositories.Cached;
using Selfix.Application.ServicesAbstractions.Statistics;
using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Statistics;
using Selfix.Shared.Extensions;
using Selfix.Application.Dtos.Statistics;

namespace Selfix.Application.UseCases.Statistics.Application;

internal sealed class GetApplicationStatisticsUseCase : IGetApplicationStatisticsUseCase
{
    private readonly IStatisticsService _statisticsService;
    private readonly IUsersRepository _usersRepository;

    public GetApplicationStatisticsUseCase(IStatisticsService statisticsService, CachedUsersRepository usersRepository)
    {
        _statisticsService = statisticsService;
        _usersRepository = usersRepository;
    }

    public IO<GetApplicationStatisticsResponse> Execute(GetApplicationStatisticsRequest request, CancellationToken cancellationToken)
    {
        return 
            from user in _usersRepository.GetById(Id<User, Ulid>.FromSafe(request.UserId), cancellationToken)
                .ToIOFailIfNone(Error.New($"User with id {request.UserId} not found"))
            from statistics in GetApplicationStatistics(user, cancellationToken)
            select new GetApplicationStatisticsResponse(statistics.ToDto());
    }

    private IO<ApplicationStatistics> GetApplicationStatistics(User user, CancellationToken cancellationToken)
    {
        return user.IsAdmin ? _statisticsService.GetApplicationStatistics(cancellationToken) : Error.New($"User with id {user.Id} is not admin");
    }
}
