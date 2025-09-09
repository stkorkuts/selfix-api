using LanguageExt;
using Selfix.Shared;

namespace Selfix.Application.Dtos.Orders;

public sealed record OrderDto(Ulid Id, OrderStatusEnum Status, Option<OrderPaymentDataDto> PaymentData);