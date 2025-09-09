using LanguageExt;
using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Orders;

namespace Selfix.Domain.Entities.Orders.Specifications;

public sealed record RestoreOrderSpecification(
    Id<Order, Ulid> Id,
    Id<User, Ulid> UserId,
    OrderStatus Status,
    OrderData Data,
    Option<OrderPaymentData> PaymentData,
    Option<Notes> Notes,
    DateTimeOffset CreatedAt);