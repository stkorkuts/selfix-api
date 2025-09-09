using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Selfix.Infrastructure.Database.Entities;

namespace Selfix.Infrastructure.Database.EntityConfigurations;

internal sealed class ImageConfiguration : IEntityTypeConfiguration<ImageDb>
{
    public void Configure(EntityTypeBuilder<ImageDb> builder)
    {
        builder.ToTable("images");

        builder.Property(b => b.Id)
            .HasColumnName("id")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(b => b.OSFilePath)
            .HasColumnType("text")
            .HasColumnName("os_file_path")
            .IsRequired();

        builder.Property(b => b.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(b => b.UserId)
            .HasColumnType("text")
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasOne(b => b.User)
            .WithMany(u => u.Images)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasKey(b => b.Id);
    }
}