namespace Selfix.Domain.ValueObjects.Telegram.Profiles.State;

public abstract record TelegramProfileState
{
    public TResult Match<TResult>(
        Func<TelegramProfileDefaultState, TResult> mapDefault,
        Func<TelegramProfileAvatarCreationState, TResult> mapAvatarCreation,
        Func<TelegramProfileImageGenerationState, TResult> mapImageGeneration) =>
        this switch
        {
            TelegramProfileDefaultState state => mapDefault(state),
            TelegramProfileAvatarCreationState state => mapAvatarCreation(state),
            TelegramProfileImageGenerationState state => mapImageGeneration(state),
            _ => throw new InvalidOperationException()
        };
}