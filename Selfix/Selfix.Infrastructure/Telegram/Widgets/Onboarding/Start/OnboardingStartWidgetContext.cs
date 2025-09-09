using Selfix.Application.Dtos;
using Selfix.Application.Dtos.TelegramProfiles;
using Selfix.Application.Dtos.Users;

namespace Selfix.Infrastructure.Telegram.Widgets.Onboarding.Start;

internal sealed record OnboardingStartWidgetContext(
    TelegramProfileDto Profile,
    UserDto User
) : WidgetContext(Profile);