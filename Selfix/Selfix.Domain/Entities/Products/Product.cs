using LanguageExt;
using LanguageExt.Common;
using Selfix.Domain.Entities.Products.Specifications;
using Selfix.Domain.ValueObjects.Common;
using Selfix.Domain.ValueObjects.Products;

namespace Selfix.Domain.Entities.Products;

// TODO: define product domain entity
public sealed class Product
{
    private Product(Id<Product, Ulid> id, ProductName name, ProductData data, Money price, Money discount)
    {
        Id = id;
        Name = name;
        Data = data;
        Price = price;
        Discount = discount;
    }

    public Id<Product, Ulid> Id { get; }
    public ProductName Name { get; }
    public ProductData Data { get; }
    public Money Price { get; }
    public Money Discount { get; }

    public static Fin<Product> New(NewProductSpecification specs)
    {
        var id = Id<Product, Ulid>.FromSafe(Ulid.NewUlid());
        if (specs.Discount >= specs.Price) return Error.New("Discount must be less than price");
        return new Product(id, specs.Name, specs.Data, specs.Price, specs.Discount);
    }
    
    public static Fin<Product> Restore(RestoreProductSpecification specs)
    {
        return new Product(specs.Id, specs.Name, specs.Data, specs.Price, specs.Discount);
    }
}