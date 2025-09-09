using LanguageExt;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;
using Selfix.Application.ServicesAbstractions.Database.Repositories;
using Selfix.Domain.Entities.Products;
using Selfix.Domain.Entities.Products.Specifications;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Products;
using Selfix.Domain.ValueObjects.Products.Packages;
using Selfix.Infrastructure.Database.Entities;
using Selfix.Shared;

namespace Selfix.Infrastructure.Database.Repositories;

internal sealed class ProductsRepository : IProductsRepository
{
    private readonly SelfixDbContext _context;

    public ProductsRepository(SelfixDbContext context)
    {
        _context = context;
    }
    
    public OptionT<IO, Product> GetById(Id<Product, Ulid> id, CancellationToken cancellationToken)
    {
        return IO<Option<Product>>.LiftAsync(async () =>
        {
            var product = await _context.Products
                .Include(p => p.Package)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive, cancellationToken);
            return product is null ? Option<Product>.None : FromDb(product).ThrowIfFail();
        });
    }

    public IO<Iterable<Product>> GetByType(ProductTypeEnum type, CancellationToken cancellationToken)
    {
        return IO<Iterable<Product>>.LiftAsync(async () =>
        {
            var products = await _context.Products
                .Include(p => p.Package)
                .OrderBy(p => p.Package.AvatarGenerationsCount * 1000 + p.Package.ImageGenerationsCount)
                .Where(p => p.Type == type && p.IsActive).ToListAsync(cancellationToken);
            return products.AsIterable().Traverse(FromDb).As().ThrowIfFail();
        });
    }

    private static Fin<Product> FromDb(ProductDb productDb)
    {
        var id = Id<Product, Ulid>.FromSafe(productDb.Id);
        return
            from productName in ProductName.From(productDb.Name)
            from data in productDb.Type switch
            {
                ProductTypeEnum.Package or ProductTypeEnum.TrialPackage or ProductTypeEnum.FirstPaymentPackage =>
                    Fin<ProductData>.Succ(new PackageProductData((uint)productDb.Package!.ImageGenerationsCount,
                        (uint)productDb.Package!.AvatarGenerationsCount)),
                ProductTypeEnum.Subscription => Error.New("Subscription product type is not supported yet"),
                _ => Error.New($"Unsupported product type: {productDb.Type.GetType().Name}")
            }
            from price in Money.From(productDb.Price)
            from discount in Money.From(productDb.Discount)
            from product in Product.Restore(new RestoreProductSpecification(id, productName, data, price, discount))
            select product;
    }
}