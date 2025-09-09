using LanguageExt;
using LanguageExt.Common;
using Selfix.Application.Dtos.Orders;
using Selfix.Application.Dtos.Products;
using Selfix.Application.Dtos.Promocodes;
using Selfix.Application.Dtos.TelegramProfiles;
using Selfix.Application.ServicesAbstractions.Database;
using Selfix.Application.ServicesAbstractions.Database.Repositories;
using Selfix.Application.ServicesAbstractions.Database.Repositories.Cached;
using Selfix.Application.ServicesAbstractions.Environment;
using Selfix.Application.ServicesAbstractions.Telegram;
using Selfix.Domain.Entities.Orders;
using Selfix.Domain.Entities.Products;
using Selfix.Domain.Entities.Promocodes;
using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Orders;
using Selfix.Domain.ValueObjects.Orders.Products;
using Selfix.Domain.ValueObjects.Orders.Promocodes;
using Selfix.Domain.ValueObjects.Products.Packages;
using Selfix.Shared;
using Selfix.Shared.Extensions;

namespace Selfix.Application.UseCases.Orders.Confirm;

internal sealed class GetOrderUseCase : IGetOrderUseCase
{
    private readonly IOrdersRepository _ordersRepository;

    public GetOrderUseCase(IOrdersRepository ordersRepository)
    {
        _ordersRepository = ordersRepository;
    }

    public IO<GetOrderResponse> Execute(GetOrderRequest request, CancellationToken cancellationToken)
    {
        return
            from order in _ordersRepository.GetById(Id<Order, Ulid>.FromSafe(request.OrderId), cancellationToken)
                .ToIOFailIfNone(Error.New($"Order with id: {request.OrderId} not found"))
            select new GetOrderResponse(order.ToDto());
    }
}