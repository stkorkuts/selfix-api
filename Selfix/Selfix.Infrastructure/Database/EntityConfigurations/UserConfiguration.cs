using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Selfix.Infrastructure.Database.Entities;
using Selfix.Shared;

namespace Selfix.Infrastructure.Database.EntityConfigurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<UserDb>
{
    public void Configure(EntityTypeBuilder<UserDb> builder)
    {
        builder.ToTable("users");

        builder.Property(b => b.Id)
            .HasColumnName("id")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(b => b.AvatarGenerationsCount)
            .HasColumnName("avatar_generations_count")
            .HasColumnType("integer")
            .IsRequired();

        builder.Property(b => b.ImageGenerationsCount)
            .HasColumnName("image_generations_count")
            .HasColumnType("integer")
            .IsRequired();

        builder.Property(b => b.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(b => b.InvitedById)
            .HasColumnName("invited_by_id")
            .HasColumnType("text")
            .IsRequired(false);
        
        builder.Property(a => a.ActiveAvatarId)
            .HasColumnName("active_avatar_id")
            .HasColumnType("text")
            .IsRequired(false);

        builder.HasOne(b => b.InvitedBy)
            .WithMany(u => u.InvitedUsers)
            .HasForeignKey(b => b.InvitedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(b => b.ActiveAvatar)
            .WithOne(a => a.ActiveForUser)
            .HasForeignKey<UserDb>(b => b.ActiveAvatarId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasKey(b => b.Id);
    }
}