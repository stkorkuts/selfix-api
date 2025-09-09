using LanguageExt;
using Selfix.Domain.Entities.TelegramProfiles;
using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Common;

namespace Selfix.Application.ServicesAbstractions.Database.Repositories;

public interface ITelegramProfilesRepository
{
    OptionT<IO, TelegramProfile> GetById(Id<TelegramProfile, long> id, CancellationToken cancellationToken);
    OptionT<IO, TelegramProfile> GetByUserId(Id<User, Ulid> id, CancellationToken cancellationToken);
    IO<Unit> Save(TelegramProfile profile, CancellationToken cancellationToken);
}