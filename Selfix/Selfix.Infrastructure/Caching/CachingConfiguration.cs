using Microsoft.Extensions.DependencyInjection;
using Selfix.Application.ServicesAbstractions.Caching;

namespace Selfix.Infrastructure.Caching;

internal static class CachingConfiguration
{
    public static IServiceCollection AddCaching(this IServiceCollection services)
    {
        return services.AddScoped<ICachingService, CachingService>();
    }
}