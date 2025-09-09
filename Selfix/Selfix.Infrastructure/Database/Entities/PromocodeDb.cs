namespace Selfix.Infrastructure.Database.Entities;

internal sealed class PromocodeDb
{
    public required Ulid Id { get; set; }
    public required string Code { get; set; }

    public Ulid? UsedByUserId { get; set; }
    public UserDb? UsedByUser { get; set; }

    public OrderDb? Order { get; set; }

    public required Ulid ProductId { get; set; }
    public ProductDb? Product { get; set; }
}