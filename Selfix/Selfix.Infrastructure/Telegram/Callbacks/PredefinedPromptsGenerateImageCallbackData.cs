using System.Text.Json.Serialization;

namespace Selfix.Infrastructure.Telegram.Callbacks;

internal sealed class PredefinedPromptsGenerateImageCallbackData : CallbackData
{
    [JsonPropertyName("p")] public required Ulid PromptId { get; init; }
    [JsonPropertyName("i")] public required uint PageIndex { get; init; }
}