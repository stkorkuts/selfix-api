using Selfix.Domain.Entities.Products;
using Selfix.Domain.ValueObjects.Common;

namespace Selfix.Domain.Entities.Promocodes.Specifications;

public sealed record NewPromocodeSpecification(Id<Product, Ulid> ProductId);