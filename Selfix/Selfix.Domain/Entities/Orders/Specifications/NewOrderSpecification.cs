using Selfix.Domain.Entities.Users;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Orders;

namespace Selfix.Domain.Entities.Orders.Specifications;

public sealed record NewOrderSpecification(Id<User, Ulid> UserId, OrderData Data, DateTimeOffset CurrentTime);