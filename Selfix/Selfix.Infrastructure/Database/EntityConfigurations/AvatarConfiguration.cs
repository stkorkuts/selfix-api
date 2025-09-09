using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Selfix.Infrastructure.Database.Entities;

namespace Selfix.Infrastructure.Database.EntityConfigurations;

internal sealed class AvatarConfiguration : IEntityTypeConfiguration<AvatarDb>
{
    public void Configure(EntityTypeBuilder<AvatarDb> builder)
    {
        builder.ToTable("avatars");

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(a => a.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(64)")
            .IsRequired();

        builder.Property(a => a.Description)
            .HasColumnName("description")
            .HasColumnType("varchar(8192)")
            .IsRequired();

        builder.Property(a => a.OSLoraFilePath)
            .HasColumnName("os_lora_file_path")
            .HasColumnType("varchar(256)")
            .IsRequired();
        
        builder.Property(o => o.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(a => a.UserId)
            .HasColumnName("user_id")
            .HasColumnType("text")
            .IsRequired();

        builder.HasOne(a => a.User)
            .WithMany(u => u.Avatars)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasKey(a => a.Id);
    }
}