using Selfix.Application.Dtos;
using Selfix.Application.Dtos.TelegramProfiles;
using Selfix.Application.Dtos.Users;

namespace Selfix.Infrastructure.Telegram.Widgets.Onboarding.Tutorial;

internal sealed record TutorialWidgetContext(
    TelegramProfileDto Profile,
    UserDto User,
    uint Step
) : WidgetContext(Profile);