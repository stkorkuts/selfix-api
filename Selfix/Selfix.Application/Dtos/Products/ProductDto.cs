using Selfix.Domain.ValueObjects.Products;

namespace Selfix.Application.Dtos.Products;

public sealed record ProductDto(Ulid Id, string Name, decimal Price, decimal Discount, ProductData Data);