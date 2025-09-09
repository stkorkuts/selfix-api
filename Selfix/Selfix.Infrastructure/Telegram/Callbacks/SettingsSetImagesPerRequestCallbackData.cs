using System.Text.Json.Serialization;

namespace Selfix.Infrastructure.Telegram.Callbacks;

internal sealed class SettingsSetImagesPerRequestCallbackData : CallbackData
{
    [JsonPropertyName("i")] public required uint ImagesPerRequest { get; set; }
}