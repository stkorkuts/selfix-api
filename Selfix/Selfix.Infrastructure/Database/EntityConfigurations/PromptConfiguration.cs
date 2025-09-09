using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Selfix.Infrastructure.Database.Entities;
using Selfix.Shared.Utils;

namespace Selfix.Infrastructure.Database.EntityConfigurations;

internal sealed class PromptConfiguration : IEntityTypeConfiguration<PromptDb>
{
    public void Configure(EntityTypeBuilder<PromptDb> builder)
    {
        builder.ToTable("prompts");

        builder.Property(b => b.Id)
            .HasColumnName("id")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(b => b.Name)
            .HasColumnName("name")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(b => b.NumberInOrder)
            .HasColumnName("number_in_order")
            .HasColumnType("integer")
            .IsRequired();

        builder.Property(b => b.Text)
            .HasColumnName("text")
            .HasColumnType("varchar(8192)")
            .IsRequired();

        builder.HasKey(b => b.Id);
    }
}