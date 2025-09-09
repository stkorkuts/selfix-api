using System.Text.Json.Serialization;
using Selfix.Shared;

namespace Selfix.Infrastructure.Telegram.Callbacks;

internal sealed class SettingsSetAspectRatioCallbackData : CallbackData
{
    [JsonPropertyName("n")] public required ImageAspectRatioEnum NewRatio { get; set; }
}