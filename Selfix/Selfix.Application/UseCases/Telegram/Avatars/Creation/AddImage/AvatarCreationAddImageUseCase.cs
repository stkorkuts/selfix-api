using LanguageExt;
using LanguageExt.Common;
using Selfix.Application.ServicesAbstractions.Database.Repositories;
using Selfix.Application.ServicesAbstractions.Database.Repositories.Cached;
using Selfix.Domain;
using Selfix.Domain.Entities.TelegramProfiles;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Telegram.Files;
using Selfix.Domain.ValueObjects.Telegram.Profiles.State;
using Selfix.Shared.Constants;
using Selfix.Shared.Extensions;

namespace Selfix.Application.UseCases.Telegram.Avatars.Creation.AddImage;

internal sealed class AvatarCreationAddImageUseCase : IAvatarCreationAddImageUseCase
{
    private readonly ITelegramProfilesRepository _telegramProfilesRepository;

    public AvatarCreationAddImageUseCase(CachedTelegramProfilesRepository telegramProfilesRepository)
    {
        _telegramProfilesRepository = telegramProfilesRepository;
    }

    public IO<AvatarCreationAddImageResponse> Execute(AvatarCreationAddImageRequest request,
        CancellationToken cancellationToken)
    {
        var profileId = Id<TelegramProfile, long>.FromSafe(request.TelegramProfileId);
        return
            from profile in _telegramProfilesRepository.GetById(profileId, cancellationToken)
                .ToIOFailIfNone(Error.New($"Profile with id: {request.TelegramProfileId} not found"))
            from response in profile.State switch
            {
                TelegramProfileAvatarCreationState state => AddImageForAvatarCreation(request.FileId, 
                        request.FileExtension, state, profile, cancellationToken)
                    .Match(
                        val => val,
                        _ => new AvatarCreationAddImageResponse((uint)state.FileIds.Count(), false, true)),
                _ => Error.New("Profile is not in avatar creation state")
            }
            select response;
    }

    private IO<AvatarCreationAddImageResponse> AddImageForAvatarCreation(string fileId,
        string fileExtension,
        TelegramProfileAvatarCreationState state,
        TelegramProfile profile,
        CancellationToken cancellationToken)
    {
        return
            from domainFileId in TelegramFile.From(fileId, fileExtension).ToIO()
            from newState in state.AddFileId(domainFileId).ToIO()
            from _1 in profile.ChangeState(newState).ToIO()
            from _2 in _telegramProfilesRepository.Save(profile, cancellationToken)
            select (uint)newState.FileIds.Count() switch
            {
                Constants.AVATAR_CREATION_IMAGES_QUANTITY => new AvatarCreationAddImageResponse(
                    (uint)newState.FileIds.Count(), true, false),
                _ => new AvatarCreationAddImageResponse(
                    (uint)newState.FileIds.Count(), false, false)
            };
    }
}