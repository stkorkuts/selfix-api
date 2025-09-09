using Selfix.Domain.Entities.Promocodes;
using Selfix.Domain.ValueObjects.Common;

namespace Selfix.Domain.ValueObjects.Orders.Promocodes;

public sealed record PromocodeOrderData(Id<Promocode, Ulid> PromocodeId) : OrderData;