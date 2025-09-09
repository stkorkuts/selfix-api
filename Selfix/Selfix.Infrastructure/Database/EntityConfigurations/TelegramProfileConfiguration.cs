using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Selfix.Infrastructure.Database.Entities;
using Selfix.Shared;

namespace Selfix.Infrastructure.Database.EntityConfigurations;

internal sealed class TelegramProfileConfiguration : IEntityTypeConfiguration<TelegramProfileDb>
{
    public void Configure(EntityTypeBuilder<TelegramProfileDb> builder)
    {
        builder.ToTable("telegram_profiles");

        builder.Property(b => b.TelegramId)
            .HasColumnName("telegram_id")
            .HasColumnType("bigint")
            .IsRequired();

        builder.Property(b => b.ProfileState)
            .HasColumnName("chat_state")
            .HasColumnType("varchar(32)")
            .HasConversion(state => state.ToString(), stateDb => Enum.Parse<TelegramProfileStateEnum>(stateDb))
            .IsRequired();

        builder.Property(b => b.Settings)
            .HasColumnName("settings")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(b => b.StateData)
            .HasColumnName("state_data")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(b => b.UserId)
            .HasColumnName("user_id")
            .HasColumnType("text")
            .IsRequired();

        builder.HasOne(b => b.User)
            .WithOne(u => u.TelegramProfile)
            .HasForeignKey<TelegramProfileDb>(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasKey(b => b.TelegramId);

        builder.HasIndex(b => b.UserId);
    }
}