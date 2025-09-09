using Microsoft.Extensions.DependencyInjection;
using Selfix.Application.ServicesAbstractions.ObjectStorage;
using Selfix.Application.ServicesAbstractions.Statistics;
using Selfix.Infrastructure.ObjectStorage;

namespace Selfix.Infrastructure.Statistics;

internal static class StatisticsServiceConfiguration
{
    public static IServiceCollection AddStatistics(this IServiceCollection services)
    {
        return services.AddScoped<IStatisticsService, StatisticsService>();
    }
}