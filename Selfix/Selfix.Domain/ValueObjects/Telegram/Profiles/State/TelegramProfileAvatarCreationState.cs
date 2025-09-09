using LanguageExt;
using LanguageExt.Common;
using Selfix.Domain.ValueObjects.Avatars;
using Selfix.Domain.ValueObjects.Telegram.Files;
using Selfix.Shared.Constants;

namespace Selfix.Domain.ValueObjects.Telegram.Profiles.State;

public sealed record TelegramProfileAvatarCreationState : TelegramProfileState
{
    private TelegramProfileAvatarCreationState(Iterable<TelegramFile> fileIds, Option<AvatarName> name)
    {
        FileIds = fileIds;
        Name = name;
    }

    public Iterable<TelegramFile> FileIds { get; }
    public Option<AvatarName> Name { get; }

    public static TelegramProfileAvatarCreationState New()
    {
        return new TelegramProfileAvatarCreationState([], Option<AvatarName>.None);
    }

    public static Fin<TelegramProfileAvatarCreationState> From(Iterable<TelegramFile> fileIds,
        Option<AvatarName> name)
    {
        if (fileIds.Count() > Constants.AVATAR_CREATION_IMAGES_QUANTITY)
            return Error.New(
                $"Avatar creation state can not have more than {Constants.AVATAR_CREATION_IMAGES_QUANTITY} files");

        return new TelegramProfileAvatarCreationState(fileIds, name);
    }

    public Fin<TelegramProfileAvatarCreationState> AddFileId(TelegramFile telegramFile)
    {
        if (FileIds.Count() >= Constants.AVATAR_CREATION_IMAGES_QUANTITY)
            return Error.New(
                $"Avatar creation state can not have more than {Constants.AVATAR_CREATION_IMAGES_QUANTITY} files");

        return From(FileIds.Add(telegramFile), Name);
    }

    public Fin<TelegramProfileAvatarCreationState> SetAvatarName(AvatarName avatarName)
    {
        return From(FileIds, avatarName);
    }

    public Fin<(Iterable<TelegramFile> FileIds, AvatarName Name)> CheckReadyForCreation()
    {
        return Name.Match(val =>
                FileIds.Count() == Constants.AVATAR_CREATION_IMAGES_QUANTITY
                    ? Fin<(Iterable<TelegramFile> FileIds, AvatarName Name)>.Succ((FileIds, val))
                    : Error.New("Not enough images to create avatar"),
            () => Error.New("Avatar name is not specified"));
    }
}