using Selfix.Application.Dtos;
using Selfix.Application.Dtos.TelegramProfiles;
using Selfix.Application.Dtos.Users;

namespace Selfix.Infrastructure.Telegram.Widgets.Avatars.Creation.ConfirmOrCancel;

internal sealed record AvatarCreationConfirmOrCancelWidgetContext(
    TelegramProfileDto Profile,
    UserDto User
) : WidgetContext(Profile);