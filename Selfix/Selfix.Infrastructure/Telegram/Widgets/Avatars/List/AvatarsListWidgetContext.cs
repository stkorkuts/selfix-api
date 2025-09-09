using LanguageExt;
using Selfix.Application.Dtos;
using Selfix.Application.Dtos.Avatars;
using Selfix.Application.Dtos.TelegramProfiles;

namespace Selfix.Infrastructure.Telegram.Widgets.Avatars.List;

internal sealed record AvatarsListWidgetContext(
    TelegramProfileDto Profile,
    Iterable<AvatarDto> Avatars
) : WidgetContext(Profile);