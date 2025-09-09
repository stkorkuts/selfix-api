using Selfix.Application.Dtos;
using Selfix.Application.Dtos.Orders;

namespace Selfix.Application.UseCases.Orders.Create;

public sealed record CreateOrderResponse(OrderDto Order);