using LanguageExt;
using Selfix.Application.UseCases.Telegram.Avatars.Creation.AddImage;
using Selfix.Application.UseCases.Telegram.Profiles.Get;
using Selfix.Infrastructure.Telegram.Widgets.Avatars.Creation.ConfirmOrCancel;
using Selfix.Infrastructure.Telegram.Widgets.Avatars.Creation.ImageAdded;
using Selfix.Shared;
using Selfix.Shared.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Selfix.Infrastructure.Telegram.Handlers;

internal sealed class PhotosHandler : IHandler
{
    private readonly IAvatarCreationAddImageUseCase _avatarCreationAddImageUseCase;
    private readonly ITelegramBotClient _botClient;
    private readonly IGetTelegramProfileWithUserUseCase _getTelegramProfileWithUserUseCase;

    public PhotosHandler(ITelegramBotClient botClient,
        IGetTelegramProfileWithUserUseCase getTelegramProfileWithUserUseCase,
        IAvatarCreationAddImageUseCase avatarCreationAddImageUseCase)
    {
        _botClient = botClient;
        _getTelegramProfileWithUserUseCase = getTelegramProfileWithUserUseCase;
        _avatarCreationAddImageUseCase = avatarCreationAddImageUseCase;
    }

    public IO<bool> TryHandle(Update update, CancellationToken cancellationToken)
    {
        if (update.Message?.Photo is null) return IO<bool>.Pure(false);
        var fileId = update.Message.Photo.Last().FileId;

        return
            from fileInfo in _botClient.GetFile(fileId, cancellationToken).ToIO()
            let fileExtension = fileInfo.FilePath?.Split('.').LastOrDefault() ?? "jpg"
            from profileResponse in _getTelegramProfileWithUserUseCase.Execute(
                new GetTelegramProfileWithUserRequest(update.Message.Chat.Id),
                cancellationToken)
            from result in profileResponse.Profile.ProfileState switch
            {
                TelegramProfileStateEnum.AvatarCreation =>
                    from response in _avatarCreationAddImageUseCase.Execute(
                        new AvatarCreationAddImageRequest(profileResponse.Profile.Id, fileId, fileExtension), cancellationToken)
                    from _1 in response.IsImageIgnored switch
                    {
                        false => from _1 in response.CanStartGeneration switch
                            {
                                true => new AvatarCreationConfirmOrCancelWidget(_botClient).Show(
                                    new AvatarCreationConfirmOrCancelWidgetContext(profileResponse.Profile,
                                        profileResponse.User), cancellationToken),
                                false => new AvatarCreationImageAddedWidget(_botClient).Show(
                                    new AvatarCreationImageAddedWidgetContext(profileResponse.Profile,
                                        response.TotalImagesUploaded), cancellationToken)
                            }
                            select Unit.Default,
                        true => IO<Unit>.Pure(Unit.Default)
                    }
                    select true,
                _ => IO<bool>.Pure(false)
            }
            select result;
    }
}