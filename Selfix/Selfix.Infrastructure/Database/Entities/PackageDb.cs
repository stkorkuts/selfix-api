namespace Selfix.Infrastructure.Database.Entities;

internal sealed class PackageDb
{
    public required Ulid Id { get; set; }
    public required int ImageGenerationsCount { get; set; }
    public required int AvatarGenerationsCount { get; set; }

    public ProductDb? Product { get; set; }
}