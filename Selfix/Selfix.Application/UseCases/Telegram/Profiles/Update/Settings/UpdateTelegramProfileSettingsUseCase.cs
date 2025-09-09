using LanguageExt;
using LanguageExt.Common;
using Selfix.Application.Dtos.TelegramProfiles;
using Selfix.Application.ServicesAbstractions.Database.Repositories;
using Selfix.Application.ServicesAbstractions.Database.Repositories.Cached;
using Selfix.Domain.Entities.TelegramProfiles;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Shared;
using Selfix.Shared.Extensions;

namespace Selfix.Application.UseCases.Telegram.Profiles.Update.Settings;

internal sealed class UpdateTelegramProfileSettingsUseCase : IUpdateTelegramProfileSettingsUseCase
{
    private readonly ITelegramProfilesRepository _telegramProfilesRepository;

    public UpdateTelegramProfileSettingsUseCase(CachedTelegramProfilesRepository telegramProfilesRepository)
    {
        _telegramProfilesRepository = telegramProfilesRepository;
    }

    public IO<UpdateTelegramProfileSettingsResponse> Execute(UpdateTelegramProfileSettingsRequest request,
        CancellationToken cancellationToken)
    {
        return
            from profile in _telegramProfilesRepository
                .GetById(Id<TelegramProfile, long>.FromSafe(request.TelegramProfileId), cancellationToken)
                .ToIOFailIfNone(Error.New($"Profile with id: {request.TelegramProfileId} not found"))
            from isDirty1 in request.TargetImagesPerRequest.Match(
                None: () => false,
                Some: some => some == profile.Settings.ImagesPerRequest
                    ? false
                    : ChangeImagesPerRequest(profile, some)).ToIO()
            from isDirty2 in request.TargetImageAspectRatio.Match(
                None: () => isDirty1,
                Some: imageAspectRatio => imageAspectRatio == profile.Settings.ImageAspectRatio
                    ? isDirty1
                    : ChangeImageAspectRatio(profile, imageAspectRatio)).ToIO()
            from _1 in isDirty2
                ? _telegramProfilesRepository.Save(profile, cancellationToken)
                : IO<Unit>.Pure(Unit.Default)
            select new UpdateTelegramProfileSettingsResponse(profile.ToDto());
    }

    private static Fin<bool> ChangeImagesPerRequest(TelegramProfile profile, uint imagesPerRequest)
    {
        return
            from imagesPerRequestDomain in NaturalNumber.From(imagesPerRequest)
            from newSettings in profile.Settings.ChangeImagesPerRequest(imagesPerRequestDomain)
            from _1 in profile.ChangeSettings(newSettings)
            select true;
    }

    private static Fin<bool> ChangeImageAspectRatio(TelegramProfile profile, ImageAspectRatioEnum imageAspectRatio)
    {
        return
            from newSettings in profile.Settings.ChangeAspectRatio(imageAspectRatio)
            from _1 in profile.ChangeSettings(newSettings)
            select true;
    }
}