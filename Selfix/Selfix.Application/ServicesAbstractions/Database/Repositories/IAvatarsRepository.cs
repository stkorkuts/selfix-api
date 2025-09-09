using LanguageExt;
using Selfix.Domain.Entities.Avatars;
using Selfix.Domain.Entities.TelegramProfiles;
using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Common;

namespace Selfix.Application.ServicesAbstractions.Database.Repositories;

public interface IAvatarsRepository
{
    OptionT<IO, Avatar> GetById(Id<Avatar, Ulid> id, CancellationToken cancellationToken);
    IO<Unit> Save(Avatar avatar, CancellationToken cancellationToken);
    IO<Iterable<Avatar>> GetUserAvatars(Id<User, Ulid> id, CancellationToken cancellationToken);
    OptionT<IO, Avatar> GetActiveByUserId(Id<User, Ulid> userId, CancellationToken cancellationToken);
}