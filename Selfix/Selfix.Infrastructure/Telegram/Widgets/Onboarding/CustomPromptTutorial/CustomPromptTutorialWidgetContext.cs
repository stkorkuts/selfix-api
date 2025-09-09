using Selfix.Application.Dtos.TelegramProfiles;
using Selfix.Application.Dtos.Users;

namespace Selfix.Infrastructure.Telegram.Widgets.Onboarding.CustomPromptTutorial;

internal sealed record CustomPromptTutorialWidgetContext(
    TelegramProfileDto Profile,
    UserDto User
) : WidgetContext(Profile);