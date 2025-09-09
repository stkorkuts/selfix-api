using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Selfix.Infrastructure.Logging;

internal static class LoggingConfiguration
{
    public static IServiceCollection AddConfiguredLogging(this IServiceCollection services, ConfigureHostBuilder host)
    {
        host.UseSerilog((context, loggerServices, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(loggerServices)
            .Enrich.FromLogContext()
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                formatProvider: CultureInfo.InvariantCulture)
        );
        // Add additional sinks and infrastructure if needed

        return services;
    }
}