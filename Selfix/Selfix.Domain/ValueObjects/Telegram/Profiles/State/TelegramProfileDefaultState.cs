namespace Selfix.Domain.ValueObjects.Telegram.Profiles.State;

public sealed record TelegramProfileDefaultState : TelegramProfileState
{
    private TelegramProfileDefaultState()
    {
    }

    public static TelegramProfileDefaultState New()
    {
        return new TelegramProfileDefaultState();
    }
}