using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Products;

namespace Selfix.Domain.Entities.Products.Specifications;

public record RestoreProductSpecification(Id<Product, Ulid> Id, ProductName Name, ProductData Data, Money Price, Money Discount);