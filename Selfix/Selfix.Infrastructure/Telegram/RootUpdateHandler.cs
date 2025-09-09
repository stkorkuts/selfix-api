using LanguageExt;
using Selfix.Infrastructure.Telegram.Handlers;
using Selfix.Infrastructure.Telegram.Utils;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace Selfix.Infrastructure.Telegram;

internal sealed class RootUpdateHandler : IUpdateHandler
{
    private readonly CallbackHandler _callbackHandler;
    private readonly DefaultHandler _defaultHandler;
    private readonly PaymentsHandler _paymentsHandler;
    private readonly ILogger _logger;
    private readonly MainCommandHandler _mainCommandHandler;
    private readonly PanelCommandHandler _panelCommandHandler;
    private readonly PhotosHandler _photosHandler;
    private readonly RegularTextHandler _regularTextHandler;

    public RootUpdateHandler(ILogger logger, MainCommandHandler mainCommandHandler,
        PanelCommandHandler panelCommandHandler, CallbackHandler callbackHandler,
        PhotosHandler photosHandler, RegularTextHandler regularTextHandler,
        DefaultHandler defaultHandler, PaymentsHandler paymentsHandler)
    {
        _logger = logger;
        _mainCommandHandler = mainCommandHandler;
        _panelCommandHandler = panelCommandHandler;
        _callbackHandler = callbackHandler;
        _photosHandler = photosHandler;
        _regularTextHandler = regularTextHandler;
        _defaultHandler = defaultHandler;
        _paymentsHandler = paymentsHandler;
    }

    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source,
        CancellationToken cancellationToken)
    {
        _logger.Information("Handle error: {Exception}", exception);
        if (exception is RequestException)
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.Information("Got update {UpdateId} data from bot: {UpdateData}", update.Id,
            update.GetShortRepresentation());

        var handlersQue = new Que<IHandler>([
            _mainCommandHandler,
            _panelCommandHandler,
            _paymentsHandler,
            _callbackHandler,
            _photosHandler,
            _regularTextHandler,
            _defaultHandler
        ]);

        var result = await HandleRecursively(handlersQue, update, cancellationToken).RunSafeAsync();

        result.Match(isHandled =>
        {
            if (isHandled)
                _logger.Information("Update {UpdateId} handled successfully", update.Id);
            else
                _logger.Warning("Unhandled update {UpdateId}", update.Id);
        }, err =>
        {
            _logger.Error(err, "Error handling update {UpdateId} with data: {UpdateData}", update.Id,
                update.GetShortRepresentation());
        });
    }

    private static IO<bool> HandleRecursively(Que<IHandler> handlers, Update update,
        CancellationToken cancellationToken)
    {
        return handlers.TryPeek().Match(
            handler =>
                from isHandled in handler.TryHandle(update, cancellationToken)
                from result in isHandled
                    ? IO<bool>.Pure(true)
                    : HandleRecursively(handlers.Dequeue(), update, cancellationToken)
                select result,
            IO<bool>.Pure(false));
    }
}