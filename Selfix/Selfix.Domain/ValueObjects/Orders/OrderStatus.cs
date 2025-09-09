using Selfix.Shared;

namespace Selfix.Domain.ValueObjects.Orders;

public sealed record OrderStatus(OrderStatusEnum Value, DateTimeOffset UpdatedAt)
{
    public bool IsCompleted => Value is OrderStatusEnum.Canceled or OrderStatusEnum.Confirmed;
}