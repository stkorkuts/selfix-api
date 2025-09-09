using Microsoft.Extensions.DependencyInjection;
using Selfix.Application.ServicesAbstractions.Environment;

namespace Selfix.Infrastructure.Environment;

internal static class EnvironmentConfiguration
{
    public static IServiceCollection AddEnvironment(this IServiceCollection services)
    {
        return services
            .AddScoped<IEnvironmentService, EnvironmentService>();
    }
}