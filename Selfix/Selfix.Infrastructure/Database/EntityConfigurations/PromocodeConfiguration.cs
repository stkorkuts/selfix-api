using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Selfix.Infrastructure.Database.Entities;

namespace Selfix.Infrastructure.Database.EntityConfigurations;

internal sealed class PromocodeConfiguration : IEntityTypeConfiguration<PromocodeDb>
{
    public void Configure(EntityTypeBuilder<PromocodeDb> builder)
    {
        builder.ToTable("promocodes");

        builder.Property(b => b.Id)
            .HasColumnName("id")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(b => b.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(32)")
            .IsRequired();

        builder.Property(b => b.UsedByUserId)
            .HasColumnName("used_by_user_id")
            .HasColumnType("text")
            .IsRequired(false);
        
        builder.HasOne(b => b.UsedByUser)
            .WithMany(b => b.UsedPromocodes)
            .HasForeignKey(b => b.UsedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Product)
            .WithMany(b => b.Promocodes)
            .HasForeignKey(b => b.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Code)
            .IsUnique();
    }
}