using Selfix.Domain.Entities.Products;
using Selfix.Domain.ValueObjects.Common;

namespace Selfix.Domain.ValueObjects.Orders.Products;

public sealed record ProductOrderData(Id<Product, Ulid> ProductId) : OrderData;