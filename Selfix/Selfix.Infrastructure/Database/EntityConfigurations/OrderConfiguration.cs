using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Selfix.Infrastructure.Database.Entities;
using Selfix.Shared;

namespace Selfix.Infrastructure.Database.EntityConfigurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<OrderDb>
{
    public void Configure(EntityTypeBuilder<OrderDb> builder)
    {
        builder.ToTable("orders");

        builder.Property(b => b.Id)
            .HasColumnName("id")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(o => o.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(16)")
            .HasConversion(status => status.ToString(), statusDb => Enum.Parse<OrderStatusEnum>(statusDb))
            .IsRequired();

        builder.Property(o => o.Type)
            .HasColumnName("type")
            .HasColumnType("varchar(16)")
            .HasConversion(type => type.ToString(), statusDb => Enum.Parse<OrderTypeEnum>(statusDb))
            .IsRequired();

        builder.Property(o => o.PaymentData)
            .HasColumnName("payment_data")
            .HasColumnType("jsonb")
            .IsRequired(false);

        builder.Property(o => o.Notes)
            .HasColumnName("notes")
            .HasColumnType("text")
            .IsRequired(false);

        builder.Property(o => o.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(o => o.UserId)
            .HasColumnName("user_id")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(o => o.ProductId)
            .HasColumnName("product_id")
            .HasColumnType("text")
            .IsRequired(false);

        builder.Property(o => o.PromocodeId)
            .HasColumnName("promocode_id")
            .HasColumnType("text")
            .IsRequired(false);

        builder.HasOne(b => b.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Product)
            .WithMany(b => b.Orders)
            .HasForeignKey(b => b.ProductId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(b => b.Promocode)
            .WithOne(b => b.Order)
            .HasForeignKey<OrderDb>(b => b.PromocodeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasKey(b => b.Id);
    }
}