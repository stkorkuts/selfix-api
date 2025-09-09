using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Selfix.Application.ServicesAbstractions.Payments;
using Selfix.Application.ServicesAbstractions.Telegram;
using Selfix.Infrastructure.Payments;
using Selfix.Infrastructure.Telegram.Handlers;
using Selfix.Infrastructure.Telegram.Services;
using Selfix.Shared.Settings;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace Selfix.Infrastructure.Telegram;

internal static class TelegramBotConfiguration
{
    public static IServiceCollection AddTelegramBot(this IServiceCollection services)
    {
        return services
            .AddTelegramBotClient()
            .AddScoped<ITelegramService, TelegramService>()
            .AddScoped<ITelegramPaymentsService, TelegramPaymentsService>()
            .AddHostedService<PollingService>()
            .AddScoped<ReceiverService>()
            .AddScoped<IUpdateHandler, RootUpdateHandler>()
            .AddHandlers();
    }

    private static IServiceCollection AddHandlers(this IServiceCollection services)
    {
        return services
            .AddScoped<MainCommandHandler>()
            .AddScoped<PanelCommandHandler>()
            .AddScoped<PaymentsHandler>()
            .AddScoped<CallbackHandler>()
            .AddScoped<PhotosHandler>()
            .AddScoped<RegularTextHandler>()
            .AddScoped<DefaultHandler>();
    }

    private static IServiceCollection AddTelegramBotClient(this IServiceCollection services)
    {
        services.AddHttpClient("TelegramBotClient")
            .RemoveAllLoggers()
            .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
            {
                var settings = sp.GetService<IOptions<TelegramBotSettings>>()?.Value;

                if (settings?.Token is null) throw new ValueIsNullException("Telegram bot token is not provided");

                TelegramBotClientOptions options = new(settings.Token);
                return new TelegramBotClient(options, httpClient);
            });
        return services;
    }
}