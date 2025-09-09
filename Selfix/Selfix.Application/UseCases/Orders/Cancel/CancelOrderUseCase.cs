using LanguageExt;
using LanguageExt.Common;
using Selfix.Application.Dtos.Orders;
using Selfix.Application.Dtos.TelegramProfiles;
using Selfix.Application.ServicesAbstractions.Database;
using Selfix.Application.ServicesAbstractions.Database.Repositories;
using Selfix.Application.ServicesAbstractions.Database.Repositories.Cached;
using Selfix.Application.ServicesAbstractions.Environment;
using Selfix.Application.ServicesAbstractions.Telegram;
using Selfix.Domain.Entities.Orders;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Orders;
using Selfix.Shared;
using Selfix.Shared.Extensions;

namespace Selfix.Application.UseCases.Orders.Cancel;

internal sealed class CancelOrderUseCase : ICancelOrderUseCase
{
    private readonly IEnvironmentService _environmentService;
    private readonly IOrdersRepository _ordersRepository;
    private readonly ITelegramProfilesRepository _telegramProfilesRepository;
    private readonly ITelegramService _telegramService;
    private readonly ITransactionService _transactionService;

    public CancelOrderUseCase(IOrdersRepository ordersRepository, IEnvironmentService environmentService,
        CachedTelegramProfilesRepository telegramProfilesRepository, ITelegramService telegramService,
        ITransactionService transactionService)
    {
        _ordersRepository = ordersRepository;
        _environmentService = environmentService;
        _telegramProfilesRepository = telegramProfilesRepository;
        _telegramService = telegramService;
        _transactionService = transactionService;
    }

    public IO<CancelOrderResponse> Execute(CancelOrderRequest request, CancellationToken cancellationToken)
    {
        return
            from order in _ordersRepository.GetById(Id<Order, Ulid>.FromSafe(request.OrderId), cancellationToken)
                .ToIOFailIfNone(Error.New($"Order with id {request.OrderId} not found"))
            from currentTime in _environmentService.GetCurrentTime(cancellationToken)
            from _1 in order.ChangeStatus(new OrderStatus(OrderStatusEnum.Canceled, currentTime)).ToIO()
            from _2 in _transactionService.Run(
                from _1 in _ordersRepository.Save(order, cancellationToken)
                from _2 in ShowTelegramOrderCanceledWidget(order, cancellationToken)
                //.IfFail(_ => IO<Unit>.Pure(Unit.Default)) - will ignore it when there are multiple profiles
                select Unit.Default, cancellationToken)
            select new CancelOrderResponse();
    }

    private IO<Unit> ShowTelegramOrderCanceledWidget(Order order, CancellationToken cancellationToken)
    {
        return
            from profile in _telegramProfilesRepository.GetByUserId(order.UserId, cancellationToken)
                .ToIOFailIfNone(Error.New($"Profile with user id: {order.UserId} not found"))
            from _1 in _telegramService.ShowOrderCanceledWidget(profile.ToDto(), order.ToDto(), cancellationToken)
            select Unit.Default;
    }
}