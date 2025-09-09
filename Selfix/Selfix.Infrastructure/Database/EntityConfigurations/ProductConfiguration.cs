using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Selfix.Infrastructure.Database.Entities;
using Selfix.Shared;
using Selfix.Shared.Utils;

namespace Selfix.Infrastructure.Database.EntityConfigurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<ProductDb>
{
    internal static readonly ProductDb[] PRODUCTS =
    [
        new ProductDb
        {
            Id = UlidUtils.NewWithSeed(100),
            Name = "Начальный пакет (1 аватар, 100 генераций)",
            Type = ProductTypeEnum.FirstPaymentPackage,
            Price = 999,
            Discount = 1000,
            IsActive = true,
            PackageId = PackageConfiguration.PACKAGES[0].Id,
        },
        new ProductDb
        {
            Id = UlidUtils.NewWithSeed(101),
            Name = "Специальное предложение (1 аватар, 20 генераций)",
            Type = ProductTypeEnum.TrialPackage,
            Price = 399,
            Discount = 1000,
            IsActive = true,
            PackageId = PackageConfiguration.PACKAGES[1].Id,
        },
        new ProductDb
        {
            Id = UlidUtils.NewWithSeed(102),
            Name = "100 генераций",
            Type = ProductTypeEnum.Package,
            Price = 999,
            Discount = 1000,
            IsActive = true,
            PackageId = PackageConfiguration.PACKAGES[2].Id,
        },
        new ProductDb
        {
            Id = UlidUtils.NewWithSeed(103),
            Name = "1 аватар",
            Type = ProductTypeEnum.Package,
            Price = 499,
            Discount = 1000,
            IsActive = true,
            PackageId = PackageConfiguration.PACKAGES[3].Id,
        }
    ];
    
    public void Configure(EntityTypeBuilder<ProductDb> builder)
    {
        builder.ToTable("products");

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(128)")
            .IsRequired();

        builder.Property(p => p.Type)
            .HasColumnType("varchar(32)")
            .HasColumnName("type")
            .HasConversion(type => type.ToString(), typeDb => Enum.Parse<ProductTypeEnum>(typeDb))
            .IsRequired();

        builder.Property(p => p.Price)
            .HasColumnName("price")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.Discount)
            .HasColumnName("discount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .HasColumnType("boolean")
            .IsRequired();
        
        builder.Property(p => p.PackageId)
            .HasColumnName("package_id")
            .HasColumnType("text")
            .IsRequired(false);
        
        builder.HasOne(b => b.Package)
            .WithOne(b => b.Product)
            .HasForeignKey<ProductDb>(b => b.PackageId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasData(PRODUCTS);
    }
}