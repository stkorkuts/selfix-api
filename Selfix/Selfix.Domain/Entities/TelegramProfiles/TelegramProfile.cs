using LanguageExt;
using LanguageExt.Common;
using Selfix.Domain.Entities.Avatars;
using Selfix.Domain.Entities.TelegramProfiles.Specifications;
using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Telegram.Profiles.Settings;
using Selfix.Domain.ValueObjects.Telegram.Profiles.State;

namespace Selfix.Domain.Entities.TelegramProfiles;

public sealed class TelegramProfile
{
    private TelegramProfile(Id<TelegramProfile, long> id, Id<User, Ulid> userId, TelegramProfileSettings settings,
        TelegramProfileState state)
    {
        Id = id;
        UserId = userId;
        Settings = settings;
        State = state;
    }

    public Id<TelegramProfile, long> Id { get; private set; }
    public Id<User, Ulid> UserId { get; }
    public TelegramProfileSettings Settings { get; private set; }
    public TelegramProfileState State { get; private set; }

    public static Fin<TelegramProfile> New(NewTelegramProfileSpecification specs)
    {
        var state = TelegramProfileDefaultState.New();
        var settings = TelegramProfileSettings.New();
        return new TelegramProfile(specs.Id, specs.UserId, settings, state);
    }

    public static Fin<TelegramProfile> Restore(RestoreTelegramProfileSpecification specs)
    {
        return new TelegramProfile(specs.Id, specs.UserId, specs.Settings, specs.State);
    }

    public Fin<Unit> ChangeState(TelegramProfileState state)
    {
        State = state;
        return Unit.Default;
    }

    public Fin<Unit> ChangeSettings(TelegramProfileSettings settings)
    {
        Settings = settings;
        return Unit.Default;
    }
}