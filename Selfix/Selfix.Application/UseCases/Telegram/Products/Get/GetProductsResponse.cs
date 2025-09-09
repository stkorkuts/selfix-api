using LanguageExt;
using Selfix.Application.Dtos;
using Selfix.Application.Dtos.Products;

namespace Selfix.Application.UseCases.Telegram.Products.Get;

public sealed record GetProductsResponse(Iterable<ProductDto> Products);