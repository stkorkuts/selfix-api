using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Common;

namespace Selfix.Domain.Entities.TelegramProfiles.Specifications;

public sealed record NewTelegramProfileSpecification(
    Id<TelegramProfile, long> Id,
    Id<User, Ulid> UserId);