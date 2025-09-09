using LanguageExt;
using LanguageExt.Common;
using Selfix.Application.Dtos.Quotas;
using Selfix.Application.Dtos.TelegramProfiles;
using Selfix.Application.Dtos.Users;
using Selfix.Application.ServicesAbstractions.Database.Repositories;
using Selfix.Application.ServicesAbstractions.Database.Repositories.Cached;
using Selfix.Application.ServicesAbstractions.Statistics;
using Selfix.Domain.Entities.TelegramProfiles;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Quotas;
using Selfix.Shared.Extensions;
using Selfix.Shared.Settings;

namespace Selfix.Application.UseCases.Telegram.Profiles.Get;

internal sealed class GetTelegramProfileWithUserUseCase : IGetTelegramProfileWithUserUseCase
{
    private readonly ITelegramProfilesRepository _telegramProfilesRepository;
    private readonly CachedUsersRepository _usersRepository;
    private readonly IStatisticsService _statisticsService;

    public GetTelegramProfileWithUserUseCase(CachedTelegramProfilesRepository telegramProfilesRepository,
        CachedUsersRepository usersRepository, IStatisticsService statisticsService)
    {
        _telegramProfilesRepository = telegramProfilesRepository;
        _usersRepository = usersRepository;
        _statisticsService = statisticsService;
    }

    public IO<GetTelegramProfileWithUserResponse> Execute(GetTelegramProfileWithUserRequest request,
        CancellationToken cancellationToken)
    {
        return
            from profile in _telegramProfilesRepository
                .GetById(Id<TelegramProfile, long>.FromSafe(request.TelegramProfileId), cancellationToken)
                .ToIOFailIfNone(Error.New($"Profile with id: {request.TelegramProfileId} not found"))
            from user in _usersRepository.GetById(profile.UserId, cancellationToken)
                .ToIOFailIfNone(Error.New($"User with id: {profile.UserId} not found"))
            from userStats in _statisticsService.GetUserStatistics(user.Id, cancellationToken)
            from quotas in UserQuotas.From(user, userStats).ToIO()
            select new GetTelegramProfileWithUserResponse(profile.ToDto(), user.ToDto(), quotas.ToDto());
    }
}