using LanguageExt;
using Selfix.Domain.Entities.Products;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Shared;

namespace Selfix.Application.ServicesAbstractions.Database.Repositories;

public interface IProductsRepository
{
    OptionT<IO, Product> GetById(Id<Product, Ulid> id, CancellationToken cancellationToken);
    IO<Iterable<Product>> GetByType(ProductTypeEnum type, CancellationToken cancellationToken);
}