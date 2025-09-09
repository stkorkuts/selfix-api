using Selfix.Application.Dtos.Orders;

namespace Selfix.Application.UseCases.Orders.Confirm;

public sealed record GetOrderResponse(OrderDto Order);