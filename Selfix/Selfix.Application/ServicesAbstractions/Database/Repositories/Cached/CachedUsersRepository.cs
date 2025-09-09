using LanguageExt;
using Selfix.Application.ServicesAbstractions.Caching;
using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Common;

namespace Selfix.Application.ServicesAbstractions.Database.Repositories.Cached;

internal sealed class CachedUsersRepository : IUsersRepository
{
    private readonly ICachingService _cachingService;
    private readonly IUsersRepository _usersRepository;

    public CachedUsersRepository(IUsersRepository usersRepository, ICachingService cachingService)
    {
        _usersRepository = usersRepository;
        _cachingService = cachingService;
    }

    public OptionT<IO, User> GetById(Id<User, Ulid> id, CancellationToken cancellationToken)
    {
        return _cachingService.Fetch(id, _usersRepository.GetById(id, cancellationToken), cancellationToken);
    }

    public IO<Unit> Save(User user, CancellationToken cancellationToken)
    {
        return _cachingService.Save(user.Id, user, _usersRepository.Save(user, cancellationToken), cancellationToken);
    }
}