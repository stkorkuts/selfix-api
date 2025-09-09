using Selfix.Domain.Entities.Orders;

namespace Selfix.Application.Dtos.Orders;

internal static class OrderMappingExtensions
{
    public static OrderDto ToDto(this Order order)
    {
        return new OrderDto(order.Id, order.Status.Value, order.PaymentData.Map(_ => new OrderPaymentDataDto()));
    }
}