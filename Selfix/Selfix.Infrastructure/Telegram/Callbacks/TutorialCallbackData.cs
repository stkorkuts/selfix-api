using System.Text.Json.Serialization;

namespace Selfix.Infrastructure.Telegram.Callbacks;

internal sealed class TutorialCallbackData : CallbackData
{
    [JsonPropertyName("s")] public required uint Step { get; init; }
}