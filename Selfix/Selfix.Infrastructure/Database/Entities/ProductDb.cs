using Selfix.Shared;

namespace Selfix.Infrastructure.Database.Entities;

internal sealed class ProductDb
{
    public required Ulid Id { get; set; }
    public required string Name { get; set; }
    public required ProductTypeEnum Type { get; set; }
    public required decimal Price { get; set; }
    public required decimal Discount { get; set; }
    public bool IsActive { get; set; }

    public Ulid? PackageId { get; set; }
    public PackageDb? Package { get; set; }

    public IList<OrderDb>? Orders { get; set; }
    public IList<PromocodeDb>? Promocodes { get; set; }
}