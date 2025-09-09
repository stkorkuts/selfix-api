namespace Selfix.Domain.ValueObjects.Products.Packages;

public sealed record PackageProductData(uint ImageGenerationsCount, uint AvatarGenerationsCount) : ProductData;