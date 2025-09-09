using LanguageExt;
using LanguageExt.Common;
using Selfix.Application.ServicesAbstractions.Database.Repositories;
using Selfix.Application.ServicesAbstractions.Database.Repositories.Cached;
using Selfix.Domain.Entities.TelegramProfiles;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Telegram.Profiles.State;
using Selfix.Shared.Extensions;

namespace Selfix.Application.UseCases.Telegram.Avatars.Creation.Cancel;

internal sealed class AvatarCreationCancelUseCase : IAvatarCreationCancelUseCase
{
    private readonly ITelegramProfilesRepository _telegramProfilesRepository;

    public AvatarCreationCancelUseCase(CachedTelegramProfilesRepository telegramProfilesRepository)
    {
        _telegramProfilesRepository = telegramProfilesRepository;
    }

    public IO<AvatarCreationCancelResponse> Execute(AvatarCreationCancelRequest request,
        CancellationToken cancellationToken)
    {
        var profileId = Id<TelegramProfile, long>.FromSafe(request.TelegramProfileId);
        return
            from profile in _telegramProfilesRepository.GetById(profileId, cancellationToken)
                .ToIOFailIfNone(Error.New($"Profile with id: {request.TelegramProfileId} not found"))
            from _1 in profile.State switch
            {
                TelegramProfileAvatarCreationState _ => CancelAvatarCreation(profile, cancellationToken),
                _ => Error.New("Profile is not in avatar creation state")
            }
            select new AvatarCreationCancelResponse();
    }

    private IO<AvatarCreationCancelResponse> CancelAvatarCreation(TelegramProfile profile,
        CancellationToken cancellationToken)
    {
        return
            from _1 in profile.ChangeState(TelegramProfileDefaultState.New()).ToIO()
            from _2 in _telegramProfilesRepository.Save(profile, cancellationToken)
            select new AvatarCreationCancelResponse();
    }
}