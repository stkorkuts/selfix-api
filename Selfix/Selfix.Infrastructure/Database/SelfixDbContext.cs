using Microsoft.EntityFrameworkCore;
using Selfix.Infrastructure.Database.Converters;
using Selfix.Infrastructure.Database.Entities;
using Selfix.Infrastructure.Database.EntityConfigurations;

namespace Selfix.Infrastructure.Database;

internal sealed class SelfixDbContext : DbContext
{
    public SelfixDbContext(DbContextOptions<SelfixDbContext> options) : base(options)
    {
    }

    public DbSet<UserDb> Users => Set<UserDb>();
    public DbSet<TelegramProfileDb> TelegramProfiles => Set<TelegramProfileDb>();
    public DbSet<PromptDb> Prompts => Set<PromptDb>();
    public DbSet<PackageDb> Packages => Set<PackageDb>();
    public DbSet<OrderDb> Orders => Set<OrderDb>();
    public DbSet<JobDb> Jobs => Set<JobDb>();
    public DbSet<AvatarDb> Avatars => Set<AvatarDb>();
    public DbSet<PromocodeDb> Promocodes => Set<PromocodeDb>();
    public DbSet<ImageDb> Images => Set<ImageDb>();
    public DbSet<ProductDb> Products => Set<ProductDb>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new TelegramProfileConfiguration());
        modelBuilder.ApplyConfiguration(new PromptConfiguration());
        modelBuilder.ApplyConfiguration(new PackageConfiguration());
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new JobConfiguration());
        modelBuilder.ApplyConfiguration(new AvatarConfiguration());
        modelBuilder.ApplyConfiguration(new PromocodeConfiguration());
        modelBuilder.ApplyConfiguration(new ImageConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<Ulid>()
            .HaveConversion<UlidToStringConverter>();
    }
}