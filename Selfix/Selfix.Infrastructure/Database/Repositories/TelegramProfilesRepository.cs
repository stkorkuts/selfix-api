using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.UnsafeValueAccess;
using Microsoft.EntityFrameworkCore;
using Selfix.Application.ServicesAbstractions.Database.Repositories;
using Selfix.Domain.Entities.TelegramProfiles;
using Selfix.Domain.Entities.TelegramProfiles.Specifications;
using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Avatars;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Telegram.Files;
using Selfix.Domain.ValueObjects.Telegram.Profiles.Settings;
using Selfix.Domain.ValueObjects.Telegram.Profiles.State;
using Selfix.Infrastructure.Database.Entities;
using Selfix.Infrastructure.Database.JsonDocumentSchema.TelegramProfile;
using Selfix.Shared;
using Selfix.Shared.Extensions;

namespace Selfix.Infrastructure.Database.Repositories;

internal sealed class TelegramProfilesRepository : ITelegramProfilesRepository
{
    private readonly SelfixDbContext _context;

    public TelegramProfilesRepository(SelfixDbContext context)
    {
        _context = context;
    }

    public OptionT<IO, TelegramProfile> GetById(Id<TelegramProfile, long> id, CancellationToken cancellationToken) =>
        Get(p => p.TelegramId == id, cancellationToken);

    public OptionT<IO, TelegramProfile> GetByUserId(Id<User, Ulid> id, CancellationToken cancellationToken) =>
        Get(p => p.UserId == id, cancellationToken);

    private OptionT<IO, TelegramProfile> Get(Expression<Func<TelegramProfileDb, bool>> filter,
        CancellationToken cancellationToken)
    {
        return
            from profileDb in OptionT<IO, TelegramProfileDb>.LiftIO(
                IO<Option<TelegramProfileDb>>.LiftAsync(() => _context.TelegramProfiles
                    .FirstOrDefaultAsync(filter, cancellationToken)
                    .Map(Prelude.Optional)))
            from profile in FromDb(profileDb).ToIO()
            select profile;
    }

    public IO<Unit> Save(TelegramProfile profile, CancellationToken cancellationToken)
    {
        return IO<Unit>.LiftAsync(async () =>
        {
            var existingProfile = await _context.TelegramProfiles.AsTracking()
                .FirstOrDefaultAsync(p => p.TelegramId == profile.Id, cancellationToken);
            var (state, data) = profile.State.Match(
                mapDefault: _ => (TelegramProfileStateEnum.Default, JsonDocument.Parse("{}")),
                mapAvatarCreation: s => (TelegramProfileStateEnum.AvatarCreation, GetAvatarCreationStateData(s)),
                mapImageGeneration: _ => (TelegramProfileStateEnum.ImageGeneration, JsonDocument.Parse("{}")));
            var settings = JsonSerializer.SerializeToDocument(
                new TelegramProfileSettingsJsonDocumentSchema(profile.Settings.ImageAspectRatio, profile.Settings.ImagesPerRequest),
                DatabaseConfiguration.JSON_SERIALIZER_OPTIONS);
            if (existingProfile is not null)
            {
                existingProfile.ProfileState = state;
                existingProfile.StateData = data;
                existingProfile.Settings = settings;
            }
            else
            {
                var newProfile = new TelegramProfileDb
                {
                    TelegramId = profile.Id,
                    ProfileState = state,
                    StateData = data,
                    Settings = settings,
                    UserId = profile.UserId
                };
                _context.TelegramProfiles.Add(newProfile);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Unit.Default;
        });
    }

    private static JsonDocument GetAvatarCreationStateData(TelegramProfileAvatarCreationState state)
    {
        return JsonSerializer.SerializeToDocument(
            new TelegramProfileAvatarCreationStateJsonDocumentSchema(
                state.FileIds.Map(tfId => (string)tfId).ToArray(),
                state.Name.Map(name => (string)name).ValueUnsafe()), 
            DatabaseConfiguration.JSON_SERIALIZER_OPTIONS);
    }

    private static Fin<TelegramProfile> FromDb(TelegramProfileDb profileDb)
    {
        var id = Id<TelegramProfile, long>.FromSafe(profileDb.TelegramId);
        var userId = Id<User, Ulid>.FromSafe(profileDb.UserId);
        return
            from settings in ParseSettings(profileDb.Settings)
            from state in ParseState(profileDb)
            from profile in TelegramProfile.Restore(
                new RestoreTelegramProfileSpecification(id, userId, settings, state))
            select profile;
    }

    private static Fin<TelegramProfileState> ParseState(TelegramProfileDb profileDb)
    {
        return profileDb.ProfileState switch
        {
            TelegramProfileStateEnum.Default => TelegramProfileDefaultState.New(),
            TelegramProfileStateEnum.ImageGeneration => TelegramProfileImageGenerationState.New(),
            TelegramProfileStateEnum.AvatarCreation => ParseAvatarCreationState(profileDb),
            _ => Error.New($"Unsupported profile state: {profileDb.ProfileState}"),
        };
    }

    private static Fin<TelegramProfileState> ParseAvatarCreationState(TelegramProfileDb profileDb)
    {
        try
        {
            var stateSchema = profileDb.StateData.Deserialize<TelegramProfileAvatarCreationStateJsonDocumentSchema>(DatabaseConfiguration.JSON_SERIALIZER_OPTIONS)!;
            return
                from avatarName in ((Option<string>)stateSchema.AvatarName).Match(
                    val => AvatarName.From(val).Map(Option<AvatarName>.Some),
                    Fin<Option<AvatarName>>.Succ(Option<AvatarName>.None))
                from telegramFileIds in stateSchema.FileIds.AsIterable().Traverse(TelegramFile.From)
                from profile in TelegramProfileAvatarCreationState.From(telegramFileIds, avatarName)
                select profile as TelegramProfileState;
        }
        catch (Exception ex)
        {
            return Error.New(ex);
        }
    }

    private static Fin<TelegramProfileSettings> ParseSettings(JsonDocument document)
    {
        try
        {
            var settingsSchema =
                document.Deserialize<TelegramProfileSettingsJsonDocumentSchema>(DatabaseConfiguration.JSON_SERIALIZER_OPTIONS)!;
            return
                from imgPerRequest in NaturalNumber.From(settingsSchema.ImagesPerRequest)
                from settings in TelegramProfileSettings.From(settingsSchema.AspectRatio, imgPerRequest)
                select settings;
        }
        catch (Exception ex)
        {
            return Error.New(ex);
        }
    }
}