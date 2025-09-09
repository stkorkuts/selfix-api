using System.Text.Json;
using System.Text.Json.Serialization;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Selfix.Infrastructure.Telegram.Utils;

internal static class UpdateExtensions
{
    private static readonly JsonSerializerOptions OPTS = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static string GetShortRepresentation(this Update update)
    {
        return update.Type switch
        {
            UpdateType.Message when update.Message is not null => GetMessageUpdateShortRepresentation(update,
                update.Message),
            UpdateType.CallbackQuery when update.CallbackQuery is not null => GetCallbackUpdateShortRepresentation(
                update, update.CallbackQuery),
            _ => JsonSerializer.Serialize(new
            {
                update.Id,
                update.Type
            }, OPTS)
        };
    }

    private static string GetMessageUpdateShortRepresentation(Update update, Message message)
    {
        return JsonSerializer.Serialize(new
        {
            update.Id,
            update.Type,
            Message = new
            {
                message.Id,
                message.Type,
                message.Text,
                Chat = new
                {
                    message.Chat.Id,
                    message.Chat.Username
                }
            }
        }, OPTS);
    }

    private static string GetCallbackUpdateShortRepresentation(Update update, CallbackQuery callback)
    {
        return JsonSerializer.Serialize(new
        {
            update.Id,
            update.Type,
            Callback = new
            {
                callback.Id,
                callback.Data,
                Chat = new
                {
                    callback.From.Id,
                    callback.From.Username
                }
            }
        }, OPTS);
    }
}