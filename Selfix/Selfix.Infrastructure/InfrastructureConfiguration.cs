using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Selfix.Infrastructure.Caching;
using Selfix.Infrastructure.Database;
using Selfix.Infrastructure.Environment;
using Selfix.Infrastructure.EventStreaming;
using Selfix.Infrastructure.Logging;
using Selfix.Infrastructure.Notifications;
using Selfix.Infrastructure.ObjectStorage;
using Selfix.Infrastructure.Payments;
using Selfix.Infrastructure.Statistics;
using Selfix.Infrastructure.Telegram;

namespace Selfix.Infrastructure;

public static class InfrastructureConfiguration
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddEnvironment()
            .AddStatistics()
            .AddConfiguredLogging(builder.Host)
            .AddDatabase()
            .AddNotifications()
            .AddCaching()
            .AddEventStreaming(builder.Configuration)
            .AddObjectStorage()
            .AddTelegramBot();
        return builder;
    }

    public static WebApplication UseInfrastructure(this WebApplication app)
    {
        if (app.Environment.EnvironmentName == "Local")
        {
            app.MigrateDatabase();
        }

        app.UseNotifications();
        
        return app;
    }
}