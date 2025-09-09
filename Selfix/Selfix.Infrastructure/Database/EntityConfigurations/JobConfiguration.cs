using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Selfix.Infrastructure.Database.Entities;
using Selfix.Shared;

namespace Selfix.Infrastructure.Database.EntityConfigurations;

internal sealed class JobConfiguration : IEntityTypeConfiguration<JobDb>
{
    public void Configure(EntityTypeBuilder<JobDb> builder)
    {
        builder.ToTable("jobs");

        builder.Property(j => j.Id)
            .HasColumnName("id")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(j => j.Type)
            .HasColumnName("type")
            .HasColumnType("varchar(32)")
            .HasConversion(type => type.ToString(), typeDb => Enum.Parse<JobTypeEnum>(typeDb))
            .IsRequired();

        builder.Property(j => j.Status)
            .HasColumnName("status")
            .HasColumnType("varchar(32)")
            .HasConversion(status => status.ToString(), statusDb => Enum.Parse<JobStatusEnum>(statusDb))
            .IsRequired();

        builder.Property(j => j.Input)
            .HasColumnName("input")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(j => j.Output)
            .HasColumnName("output")
            .HasColumnType("jsonb")
            .IsRequired(false);

        builder.Property(j => j.Notes)
            .HasColumnName("notes")
            .HasColumnType("text")
            .IsRequired(false);

        builder.Property(j => j.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(j => j.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(j => j.UserId)
            .HasColumnName("user_id")
            .HasColumnType("text")
            .IsRequired();

        builder.HasOne(j => j.User)
            .WithMany(u => u.Jobs)
            .HasForeignKey(j => j.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasKey(j => j.Id);

        builder.HasIndex(j => j.UserId);
    }
}