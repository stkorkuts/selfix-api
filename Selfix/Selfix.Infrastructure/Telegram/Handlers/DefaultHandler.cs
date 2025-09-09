using LanguageExt;
using Selfix.Infrastructure.Telegram.Utils;
using Selfix.Shared.Extensions;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Selfix.Infrastructure.Telegram.Handlers;

internal sealed class DefaultHandler : IHandler
{
    private readonly ITelegramBotClient _client;
    private readonly ILogger _logger;

    public DefaultHandler(ITelegramBotClient client, ILogger logger)
    {
        _client = client;
        _logger = logger;
    }

    public IO<bool> TryHandle(Update update, CancellationToken cancellationToken)
    {
        _logger.Warning("Unhandled update {UpdateId} with data: {UpdateData}", update.Id,
            update.GetShortRepresentation());

        return update.Type switch
        {
            UpdateType.Message when update.Message is not null => _client.SendMessage(update.Message.Chat.Id,
                "Я не знаю как обработать это сообщение", cancellationToken: cancellationToken).ToIO().Map(_ => true),
            UpdateType.InlineQuery when update.InlineQuery is not null => _client
                .SendMessage(update.InlineQuery.From.Id, "Я не знаю как обработать это сообщение",
                    cancellationToken: cancellationToken).ToIO().Map(_ => true),
            _ => IO<bool>.Pure(false)
        };
    }
}