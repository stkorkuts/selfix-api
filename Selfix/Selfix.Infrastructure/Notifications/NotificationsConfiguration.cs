using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Selfix.Application.ServicesAbstractions.Notifications;
using Selfix.Shared.Settings;

namespace Selfix.Infrastructure.Notifications;

public static class NotificationsConfiguration
{
    public static IServiceCollection AddNotifications(this IServiceCollection services)
    {
        return services
            .AddScoped<INotificationsService, NotificationsService>()
            .AddScoped<NotificationsExecutor>()
            .AddHangfire((sp, configuration) =>
            {
                var notificationsSettings = sp.GetRequiredService<IOptions<NotificationsSettings>>().Value;
                
                configuration
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UsePostgreSqlStorage(options => 
                    {
                        options.UseNpgsqlConnection(notificationsSettings.DbConnectionString);
                    }, new PostgreSqlStorageOptions
                    {
                        PrepareSchemaIfNecessary = true
                    })
                    .UseFilter(new AutomaticRetryAttribute { Attempts = 3, DelaysInSeconds = [10, 60, 3600] });
            })
            .AddHangfireServer();
    }
    
    public static WebApplication UseNotifications(this WebApplication app)
    {
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = [new HangfireBasicAuthFilter()],
            DashboardTitle = "Selfix Background Jobs"
        });
        
        return app;
    }
}