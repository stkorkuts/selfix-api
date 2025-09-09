using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Selfix.Shared.Settings;

namespace Selfix.EntryPoint;

internal static class AppBuilderExtensions
{
    public static WebApplicationBuilder AddSettings(this WebApplicationBuilder webApplicationBuilder)
    {
        return webApplicationBuilder
            .AddSettings<AdministrationSettings>(AdministrationSettings.KEY)
            .AddSettings<AssetsSettings>(AssetsSettings.KEY)
            .AddSettings<TelegramBotSettings>(TelegramBotSettings.KEY)
            .AddSettings<DatabaseSettings>(DatabaseSettings.KEY)
            .AddSettings<NotificationsSettings>(NotificationsSettings.KEY)
            .AddSettings<TelegramBotPaymentsSettings>(TelegramBotPaymentsSettings.KEY)
            .AddSettings<EventStreamingSettings>(EventStreamingSettings.KEY)
            .AddSettings<CachingSettings>(CachingSettings.KEY)
            .AddSettings<ObjectStorageSettings>(ObjectStorageSettings.KEY)
            .AddSettings<CustomLoggingSettings>(CustomLoggingSettings.KEY);
    }

    private static WebApplicationBuilder AddSettings<T>(this WebApplicationBuilder webApplicationBuilder, string key)
        where T : class
    {
        webApplicationBuilder.Services
            .AddOptions<T>().Bind(webApplicationBuilder.Configuration.GetSection(key))
            .ValidateDataAnnotations().ValidateOnStart();
        return webApplicationBuilder;
    }
}