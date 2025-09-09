using Serilog;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace Selfix.Infrastructure.Telegram.Services;

internal sealed class ReceiverService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger _logger;
    private readonly IUpdateHandler _updateHandler;

    public ReceiverService(ITelegramBotClient botClient, IUpdateHandler updateHandler, ILogger logger)
    {
        _botClient = botClient;
        _updateHandler = updateHandler;
        _logger = logger;
    }

    public async Task ReceiveAsync(CancellationToken cancellationToken)
    {
        var receiverOptions = new ReceiverOptions { DropPendingUpdates = true, AllowedUpdates = [] };

        var me = await _botClient.GetMe(cancellationToken);
        _logger.Information("Start receiving updates for {BotName}", me.Username ?? "My Awesome Bot");

        await _botClient.ReceiveAsync(_updateHandler, receiverOptions, cancellationToken);
    }
}