using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Selfix.Infrastructure.Database.Entities;
using Selfix.Shared.Utils;

namespace Selfix.Infrastructure.Database.EntityConfigurations;

internal sealed class PackageConfiguration : IEntityTypeConfiguration<PackageDb>
{
    internal static readonly PackageDb[] PACKAGES =
    [
        new PackageDb
        {
            Id = UlidUtils.NewWithSeed(0),
            AvatarGenerationsCount = 1,
            ImageGenerationsCount = 100
        },
        new PackageDb
        {
            Id = UlidUtils.NewWithSeed(1),
            AvatarGenerationsCount = 1,
            ImageGenerationsCount = 20
        },
        new PackageDb
        {
            Id = UlidUtils.NewWithSeed(2),
            AvatarGenerationsCount = 0,
            ImageGenerationsCount = 100
        },
        new PackageDb
        {
            Id = UlidUtils.NewWithSeed(3),
            AvatarGenerationsCount = 1,
            ImageGenerationsCount = 0
        }
    ];
    
    public void Configure(EntityTypeBuilder<PackageDb> builder)
    {
        builder.ToTable("packages");

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(p => p.ImageGenerationsCount)
            .HasColumnName("image_generations_count")
            .HasColumnType("integer")
            .IsRequired();

        builder.Property(p => p.AvatarGenerationsCount)
            .HasColumnName("avatar_generations_count")
            .HasColumnType("integer")
            .IsRequired();
        
        builder.HasData(PACKAGES);
    }
}