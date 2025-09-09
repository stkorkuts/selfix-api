using System.Text.Json.Serialization;

namespace Selfix.Infrastructure.Telegram.Callbacks;

internal sealed class PredefinedPromptsSwitchPageCallbackData : CallbackData
{
    [JsonPropertyName("t")] public required uint TargetPageIndex { get; init; }
}