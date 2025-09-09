using Selfix.Domain.Entities.Products;

namespace Selfix.Application.Dtos.Products;

internal static class ProductMappingExtensions
{
    public static ProductDto ToDto(this Product product)
    {
        return new ProductDto(product.Id, product.Name, product.Price, product.Discount, product.Data);
    }
}