using System.Text.Json.Serialization;

namespace Selfix.Infrastructure.Telegram.Callbacks;

internal sealed class SetActiveAvatarCallbackData : CallbackData
{
    [JsonPropertyName("a")] public required Ulid AvatarId { get; init; }
}