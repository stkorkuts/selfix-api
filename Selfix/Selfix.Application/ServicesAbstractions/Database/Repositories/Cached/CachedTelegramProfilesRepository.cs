using LanguageExt;
using Selfix.Application.ServicesAbstractions.Caching;
using Selfix.Domain.Entities.TelegramProfiles;
using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Common;

namespace Selfix.Application.ServicesAbstractions.Database.Repositories.Cached;

internal sealed class CachedTelegramProfilesRepository : ITelegramProfilesRepository
{
    private readonly ICachingService _cachingService;
    private readonly ITelegramProfilesRepository _telegramProfilesRepository;

    public CachedTelegramProfilesRepository(ITelegramProfilesRepository telegramProfilesRepository,
        ICachingService cachingService)
    {
        _telegramProfilesRepository = telegramProfilesRepository;
        _cachingService = cachingService;
    }

    public OptionT<IO, TelegramProfile> GetById(Id<TelegramProfile, long> id, CancellationToken cancellationToken)
    {
        return _cachingService.Fetch(id, _telegramProfilesRepository.GetById(id, cancellationToken), cancellationToken);
    }

    public OptionT<IO, TelegramProfile> GetByUserId(Id<User, Ulid> id, CancellationToken cancellationToken)
    {
        return _telegramProfilesRepository.GetByUserId(id, cancellationToken);
    }

    public IO<Unit> Save(TelegramProfile profile, CancellationToken cancellationToken)
    {
        return _cachingService.Save(profile.Id, profile, _telegramProfilesRepository.Save(profile, cancellationToken),
            cancellationToken);
    }
}