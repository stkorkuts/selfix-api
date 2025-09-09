namespace Selfix.Domain.ValueObjects.Telegram.Profiles.State;

public sealed record TelegramProfileImageGenerationState : TelegramProfileState
{
    private TelegramProfileImageGenerationState()
    {
    }

    public static TelegramProfileImageGenerationState New()
    {
        return new TelegramProfileImageGenerationState();
    }
}