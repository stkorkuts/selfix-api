using System.Text.Json;
using LanguageExt;
using LanguageExt.Common;
using Selfix.Application.UseCases.Orders.Cancel;
using Selfix.Application.UseCases.Orders.Confirm;
using Selfix.Infrastructure.Telegram.Payments.Schema;
using Selfix.Shared;
using Selfix.Shared.Extensions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.Payments;

namespace Selfix.Infrastructure.Telegram.Handlers;

public class PaymentsHandler : IHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly IConfirmOrderUseCase _confirmOrderUseCase;
    private readonly ICancelOrderUseCase _cancelOrderUseCase;
    private readonly IGetOrderUseCase _getOrderUseCase;

    public PaymentsHandler(ITelegramBotClient botClient, IConfirmOrderUseCase confirmOrderUseCase,
        ICancelOrderUseCase cancelOrderUseCase, IGetOrderUseCase getOrderUseCase)
    {
        _botClient = botClient;
        _confirmOrderUseCase = confirmOrderUseCase;
        _cancelOrderUseCase = cancelOrderUseCase;
        _getOrderUseCase = getOrderUseCase;
    }

    public IO<bool> TryHandle(Update update, CancellationToken cancellationToken)
    {
        return update.Type switch
        {
            UpdateType.PreCheckoutQuery when update.PreCheckoutQuery is not null => HandlePreCheckoutQuery(
                update.PreCheckoutQuery, cancellationToken),
            UpdateType.Message when update.Message?.SuccessfulPayment is not null => HandleSuccessfulPayment(update.Message.SuccessfulPayment, cancellationToken),
            UpdateType.Message when update.Message?.RefundedPayment is not null => HandleRefundedPayment(update.Message.RefundedPayment, cancellationToken),
            _ => IO<bool>.Pure(false)
        };
    }

    private IO<bool> HandlePreCheckoutQuery(PreCheckoutQuery preCheckoutQuery, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Deserialize<OrderPayload>(preCheckoutQuery.InvoicePayload)!;
        return
            from order in _getOrderUseCase.Execute(new GetOrderRequest(payload.OrderId), cancellationToken)
            from _ in order.Order.Status switch
            {
                OrderStatusEnum.Processing => _botClient
                    .AnswerPreCheckoutQuery(preCheckoutQuery.Id, cancellationToken: cancellationToken)
                    .ToIO(),
                _ => _botClient
                    .AnswerPreCheckoutQuery(preCheckoutQuery.Id, "Этот заказ уже обработан", cancellationToken: cancellationToken)
                    .ToIO()
            }
            select true;
    }
    
    private IO<bool> HandleSuccessfulPayment(SuccessfulPayment payment, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Deserialize<OrderPayload>(payment.InvoicePayload)!;
        return _confirmOrderUseCase.Execute(new ConfirmOrderRequest(payload.OrderId), cancellationToken).Map(_ => true);
    }
    
    private IO<bool> HandleRefundedPayment(RefundedPayment payment, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Deserialize<OrderPayload>(payment.InvoicePayload)!;
        return _cancelOrderUseCase.Execute(new CancelOrderRequest(payload.OrderId), cancellationToken).Map(_ => true);
    }
}