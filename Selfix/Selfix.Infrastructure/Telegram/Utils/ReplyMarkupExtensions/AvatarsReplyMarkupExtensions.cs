using LanguageExt;
using Selfix.Application.Dtos;
using Selfix.Application.Dtos.Avatars;
using Selfix.Infrastructure.Telegram.Callbacks;
using Telegram.Bot.Types.ReplyMarkups;

namespace Selfix.Infrastructure.Telegram.Utils.ReplyMarkupExtensions;

internal static class AvatarsReplyMarkupExtensions
{
    public static Fin<InlineKeyboardMarkup> GetReplyMarkup(this Iterable<AvatarDto> avatars)
    {
        return
            from avatarsButtons in GetAvatarsButtons(avatars)
            from commandButtons in GetCommandButtons()
            let buttons = avatarsButtons.Concat(commandButtons).Map(b => new[] { b })
            select new InlineKeyboardMarkup(buttons);
    }

    private static Fin<Iterable<InlineKeyboardButton>> GetAvatarsButtons(Iterable<AvatarDto> avatars)
    {
        var avatarsWithCallbacks =
            (from a in avatars.Traverse(a => CallbackSerializer.Serialize(new SetActiveAvatarCallbackData
                {
                    AvatarId = a.Id
                }))
                select a.Zip(avatars))
            .As();

        return
            from a in avatarsWithCallbacks
            let b = a.Map(avatarWithCallback =>
                InlineKeyboardButton.WithCallbackData(avatarWithCallback.Second.Name, avatarWithCallback.First))
            select b;
    }

    private static Fin<InlineKeyboardButton[]> GetCommandButtons()
    {
        return
            from callbackData in CallbackSerializer.Serialize(new CreateAvatarCallbackData())
            let addAvatarButton = InlineKeyboardButton.WithCallbackData("Создать аватар", callbackData)
            select new[] { addAvatarButton };
    }
}