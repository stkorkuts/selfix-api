using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Telegram.Profiles.Settings;
using Selfix.Domain.ValueObjects.Telegram.Profiles.State;

namespace Selfix.Domain.Entities.TelegramProfiles.Specifications;

public sealed record RestoreTelegramProfileSpecification(
    Id<TelegramProfile, long> Id,
    Id<User, Ulid> UserId,
    TelegramProfileSettings Settings,
    TelegramProfileState State);