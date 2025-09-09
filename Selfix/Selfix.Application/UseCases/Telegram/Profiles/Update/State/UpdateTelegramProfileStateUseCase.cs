using LanguageExt;
using LanguageExt.Common;
using Selfix.Application.Dtos.TelegramProfiles;
using Selfix.Application.ServicesAbstractions.Database.Repositories;
using Selfix.Application.ServicesAbstractions.Database.Repositories.Cached;
using Selfix.Domain.Entities.TelegramProfiles;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Telegram.Profiles.State;
using Selfix.Shared;
using Selfix.Shared.Extensions;

namespace Selfix.Application.UseCases.Telegram.Profiles.Update.State;

internal sealed class UpdateTelegramProfileStateUseCase : IUpdateTelegramProfileStateUseCase
{
    private readonly ITelegramProfilesRepository _telegramProfilesRepository;

    public UpdateTelegramProfileStateUseCase(CachedTelegramProfilesRepository telegramProfilesRepository)
    {
        _telegramProfilesRepository = telegramProfilesRepository;
    }

    public IO<UpdateTelegramProfileStateResponse> Execute(UpdateTelegramProfileStateRequest request,
        CancellationToken cancellationToken)
    {
        return
            from profile in _telegramProfilesRepository
                .GetById(Id<TelegramProfile, long>.FromSafe(request.TelegramProfileId), cancellationToken)
                .ToIOFailIfNone(Error.New($"Profile with id: {request.TelegramProfileId} not found"))
            from _1 in ChangeProfileState(profile, request.TargetState, cancellationToken)
            select new UpdateTelegramProfileStateResponse(profile.ToDto());
    }

    private IO<Unit> ChangeProfileState(TelegramProfile profile, TelegramProfileStateEnum targetState,
        CancellationToken cancellationToken)
    {
        TelegramProfileState stateToChange = targetState switch
        {
            TelegramProfileStateEnum.Default => TelegramProfileDefaultState.New(),
            TelegramProfileStateEnum.AvatarCreation => TelegramProfileAvatarCreationState.New(),
            TelegramProfileStateEnum.ImageGeneration => TelegramProfileImageGenerationState.New(),
            _ => throw new ArgumentOutOfRangeException($"Unsupported target state: {targetState}")
        };

        return 
            from _1 in profile.ChangeState(stateToChange).ToIO()
            from _2 in _telegramProfilesRepository.Save(profile, cancellationToken)
            select Unit.Default;
    }
}