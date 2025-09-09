using LanguageExt;
using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Common;

namespace Selfix.Application.ServicesAbstractions.Database.Repositories;

public interface IUsersRepository
{
    OptionT<IO, User> GetById(Id<User, Ulid> id, CancellationToken cancellationToken);
    IO<Unit> Save(User user, CancellationToken cancellationToken);
}