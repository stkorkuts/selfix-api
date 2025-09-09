using LanguageExt;
using LanguageExt.Common;
using Selfix.Application.ServicesAbstractions.Database.Repositories;
using Selfix.Application.ServicesAbstractions.Database.Repositories.Cached;
using Selfix.Domain.Entities.TelegramProfiles;
using Selfix.Domain.ValueObjects.Avatars;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Telegram.Profiles.State;
using Selfix.Shared.Extensions;

namespace Selfix.Application.UseCases.Telegram.Avatars.Creation.SetName;

internal sealed class AvatarCreationSetAvatarNameUseCase : IAvatarCreationSetAvatarNameUseCase
{
    private readonly ITelegramProfilesRepository _telegramProfilesRepository;

    public AvatarCreationSetAvatarNameUseCase(CachedTelegramProfilesRepository telegramProfilesRepository)
    {
        _telegramProfilesRepository = telegramProfilesRepository;
    }

    public IO<AvatarCreationSetAvatarNameResponse> Execute(AvatarCreationSetAvatarNameRequest request,
        CancellationToken cancellationToken)
    {
        var profileId = Id<TelegramProfile, long>.FromSafe(request.TelegramProfileId);
        return
            from profile in _telegramProfilesRepository.GetById(profileId, cancellationToken)
                .ToIOFailIfNone(Error.New($"Profile with id: {request.TelegramProfileId} not found"))
            from _1 in profile.State switch
            {
                TelegramProfileAvatarCreationState state => SetAvatarName(profile, state, request.Name,
                    cancellationToken),
                _ => Error.New($"Profile with id: {request.TelegramProfileId} is not in avatar creation state")
            }
            select new AvatarCreationSetAvatarNameResponse();
    }

    private IO<AvatarCreationSetAvatarNameResponse> SetAvatarName(TelegramProfile profile,
        TelegramProfileAvatarCreationState state,
        string newName,
        CancellationToken cancellationToken)
    {
        return
            from nameDomain in AvatarName.From(newName).ToIO()
            from newState in state.SetAvatarName(nameDomain).ToIO()
            from _1 in profile.ChangeState(newState).ToIO()
            from _2 in _telegramProfilesRepository.Save(profile, cancellationToken)
            select new AvatarCreationSetAvatarNameResponse();
    }
}