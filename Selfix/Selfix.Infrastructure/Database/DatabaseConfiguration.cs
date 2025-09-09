using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Selfix.Application.ServicesAbstractions.Database;
using Selfix.Application.ServicesAbstractions.Database.Repositories;
using Selfix.Infrastructure.Database.Repositories;
using Selfix.Shared;
using Selfix.Shared.Settings;

namespace Selfix.Infrastructure.Database;

internal static class DatabaseConfiguration
{
    internal static readonly JsonSerializerOptions JSON_SERIALIZER_OPTIONS = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter<ImageAspectRatioEnum>()
        },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    
    public static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        return services
            .AddDbContextPool()
            .AddScoped<ITransactionService, TransactionService>()
            .AddRepositories();
    }

    public static WebApplication MigrateDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SelfixDbContext>();
        dbContext.Database.Migrate();
        return app;
    }

    private static IServiceCollection AddDbContextPool(this IServiceCollection services)
    {
        services.AddDbContextPool<SelfixDbContext>((sp, optionsBuilder) =>
        {
            var settings = sp.GetService<IOptions<DatabaseSettings>>()!.Value;

            optionsBuilder
                .UseNpgsql(settings.ConnectionString)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        return services
            .AddScoped<IAvatarsRepository, AvatarsRepository>()
            .AddScoped<IImagesRepository, ImagesRepository>()
            .AddScoped<IJobsRepository, JobsRepository>()
            .AddScoped<IOrdersRepository, OrdersRepository>()
            .AddScoped<IProductsRepository, ProductsRepository>()
            .AddScoped<IPromocodesRepository, PromocodesRepository>()
            .AddScoped<IPromptsRepository, PromptsRepository>()
            .AddScoped<ITelegramProfilesRepository, TelegramProfilesRepository>()
            .AddScoped<IUsersRepository, UsersRepository>();
    }
}