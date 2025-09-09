using System.Text.Json;
using LanguageExt;
using LanguageExt.Common;

namespace Selfix.Infrastructure.Telegram.Callbacks;

internal static class CallbackSerializer
{
    private const string TUTORIAL_CALLBACK = "tuto";
    private const string PREDEFINED_PROMPTS_SWITCH_PAGE_CALLBACK = "ppsp";
    private const string PREDEFINED_PROMPTS_GENERATE_IMAGE_CALLBACK = "ppgi";
    private const string CREATE_AVATAR_CALLBACK = "crav";
    private const string SET_ACTIVE_AVATAR_CALLBACK = "sava";
    private const string SHOW_PRODUCTS_CALLBACK = "shpr";
    private const string CANCEL_AVATAR_CREATION_CALLBACK = "cava";
    private const string INVITE_USER_CALLBACK = "inus";
    private const string BUY_PROMOCODE_CALLBACK = "bupr";
    private const string SETTINGS_SHOW_ASPECT_RATIO_OPTIONS_CALLBACK = "cars";
    private const string SETTINGS_SHOW_IMAGES_PER_REQUEST_OPTIONS_CALLBACK = "cipr";
    private const string BUY_PACKAGE_CALLBACK = "bupa";
    private const string CUSTOM_PROMPT_TUTORIAL_CALLBACK = "cptu";
    private const string SETTINGS_SET_ASPECT_RATIO_CALLBACK = "ssar";
    private const string SETTINGS_SET_IMAGES_PER_REQUEST_CALLBACK = "ssip";
    private const string CONFIRM_AVATAR_CREATION_CALLBACK = "cacr";

    public static Fin<CallbackData> Deserialize(string value)
    {
        var parts = value.Split([':'], 2);
        if (parts.Length != 2)
            return Error.New(new ArgumentException("Invalid callback data", nameof(value)));

        return parts[0] switch
        {
            TUTORIAL_CALLBACK => TryDeserialize<TutorialCallbackData>(parts[1]),
            PREDEFINED_PROMPTS_SWITCH_PAGE_CALLBACK =>
                TryDeserialize<PredefinedPromptsSwitchPageCallbackData>(parts[1]),
            PREDEFINED_PROMPTS_GENERATE_IMAGE_CALLBACK => TryDeserialize<PredefinedPromptsGenerateImageCallbackData>(
                parts[1]),
            CREATE_AVATAR_CALLBACK => TryDeserialize<CreateAvatarCallbackData>(parts[1]),
            SET_ACTIVE_AVATAR_CALLBACK => TryDeserialize<SetActiveAvatarCallbackData>(parts[1]),
            SHOW_PRODUCTS_CALLBACK => TryDeserialize<ShowProductsCallbackData>(parts[1]),
            CANCEL_AVATAR_CREATION_CALLBACK => TryDeserialize<CancelAvatarCreationCallbackData>(parts[1]),
            INVITE_USER_CALLBACK => TryDeserialize<InviteUserCallbackData>(parts[1]),
            BUY_PROMOCODE_CALLBACK => TryDeserialize<BuyPromocodeCallbackData>(parts[1]),
            SETTINGS_SHOW_ASPECT_RATIO_OPTIONS_CALLBACK => TryDeserialize<SettingsShowAspectRatioOptionsCallbackData>(
                parts[1]),
            SETTINGS_SHOW_IMAGES_PER_REQUEST_OPTIONS_CALLBACK =>
                TryDeserialize<SettingsShowImagesPerRequestOptionsCallbackData>(
                    parts[1]),
            BUY_PACKAGE_CALLBACK => TryDeserialize<BuyPackageCallbackData>(parts[1]),
            CUSTOM_PROMPT_TUTORIAL_CALLBACK => TryDeserialize<CustomPromptTutorialCallbackData>(parts[1]),
            SETTINGS_SET_ASPECT_RATIO_CALLBACK => TryDeserialize<SettingsSetAspectRatioCallbackData>(parts[1]),
            SETTINGS_SET_IMAGES_PER_REQUEST_CALLBACK => TryDeserialize<SettingsSetImagesPerRequestCallbackData>(
                parts[1]),
            CONFIRM_AVATAR_CREATION_CALLBACK => TryDeserialize<ConfirmAvatarCreationCallbackData>(parts[1]),
            _ => Error.New(new NotSupportedException($"Unsupported callback type: {parts[0]}"))
        };
    }

    private static Fin<CallbackData> TryDeserialize<T>(string payload) where T : CallbackData
    {
        try
        {
            return JsonSerializer.Deserialize<T>(payload) ??
                   throw new ArgumentException("Invalid payload", nameof(payload));
        }
        catch (Exception ex)
        {
            return Error.New(ex);
        }
    }

    public static Fin<string> Serialize<T>(T value) where T : CallbackData
    {
        try
        {
            return value switch
            {
                TutorialCallbackData val => $"{TUTORIAL_CALLBACK}:{JsonSerializer.Serialize(val)}",
                PredefinedPromptsSwitchPageCallbackData val =>
                    $"{PREDEFINED_PROMPTS_SWITCH_PAGE_CALLBACK}:{JsonSerializer.Serialize(val)}",
                PredefinedPromptsGenerateImageCallbackData val =>
                    $"{PREDEFINED_PROMPTS_GENERATE_IMAGE_CALLBACK}:{JsonSerializer.Serialize(val)}",
                CreateAvatarCallbackData val => $"{CREATE_AVATAR_CALLBACK}:{JsonSerializer.Serialize(val)}",
                SetActiveAvatarCallbackData val => $"{SET_ACTIVE_AVATAR_CALLBACK}:{JsonSerializer.Serialize(val)}",
                ShowProductsCallbackData val => $"{SHOW_PRODUCTS_CALLBACK}:{JsonSerializer.Serialize(val)}",
                CancelAvatarCreationCallbackData val =>
                    $"{CANCEL_AVATAR_CREATION_CALLBACK}:{JsonSerializer.Serialize(val)}",
                InviteUserCallbackData val => $"{INVITE_USER_CALLBACK}:{JsonSerializer.Serialize(val)}",
                BuyPromocodeCallbackData val => $"{BUY_PROMOCODE_CALLBACK}:{JsonSerializer.Serialize(val)}",
                SettingsShowAspectRatioOptionsCallbackData val =>
                    $"{SETTINGS_SHOW_ASPECT_RATIO_OPTIONS_CALLBACK}:{JsonSerializer.Serialize(val)}",
                SettingsShowImagesPerRequestOptionsCallbackData val =>
                    $"{SETTINGS_SHOW_IMAGES_PER_REQUEST_OPTIONS_CALLBACK}:{JsonSerializer.Serialize(val)}",
                BuyPackageCallbackData val => $"{BUY_PACKAGE_CALLBACK}:{JsonSerializer.Serialize(val)}",
                CustomPromptTutorialCallbackData val =>
                    $"{CUSTOM_PROMPT_TUTORIAL_CALLBACK}:{JsonSerializer.Serialize(val)}",
                SettingsSetAspectRatioCallbackData val =>
                    $"{SETTINGS_SET_ASPECT_RATIO_CALLBACK}:{JsonSerializer.Serialize(val)}",
                SettingsSetImagesPerRequestCallbackData val =>
                    $"{SETTINGS_SET_IMAGES_PER_REQUEST_CALLBACK}:{JsonSerializer.Serialize(val)}",
                ConfirmAvatarCreationCallbackData val =>
                    $"{CONFIRM_AVATAR_CREATION_CALLBACK}:{JsonSerializer.Serialize(val)}",
                _ => Error.New(new NotSupportedException($"Unsupported callback type: {value.GetType()}"))
            };
        }
        catch (Exception ex)
        {
            return Error.New(ex);
        }
    }
}