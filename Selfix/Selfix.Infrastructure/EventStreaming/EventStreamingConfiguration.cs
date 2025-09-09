using System.Text.Json.Serialization;
using Confluent.Kafka;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Selfix.Application.ServicesAbstractions.EventStreaming;
using Selfix.Infrastructure.EventStreaming.Consumers;
using Selfix.Schema.Kafka.Jobs.Avatars.V1.AvatarCreation;
using Selfix.Schema.Kafka.Jobs.Images.V1.ImageGeneration;
using Selfix.Schema.Kafka.Jobs.Images.V1.PromptProcessing;
using Selfix.Shared.Constants;
using Selfix.Shared.Settings;
using Serilog;

namespace Selfix.Infrastructure.EventStreaming;

internal static class EventStreamingConfiguration
{
    public static IServiceCollection AddEventStreaming(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddConfiguredMassTransit(configuration)
            .AddScoped<IEventStreamingService, EventStreamingService>();
    }

    private static IServiceCollection AddConfiguredMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddMassTransit(massTransitOptions =>
        {
            massTransitOptions.SetKebabCaseEndpointNameFormatter();
            massTransitOptions.AddSerilog();
            massTransitOptions.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            massTransitOptions.AddConsumer<AvatarCreationResultJobConsumer>();
            massTransitOptions.AddConsumer<PromptProcessingResultJobConsumer>();
            massTransitOptions.AddConsumer<ImagesGenerationResultJobConsumer>();

            massTransitOptions.UsingInMemory((context, cfg) =>
            {
                cfg.UseInstrumentation(serviceName: Constants.APPLICATION_NAME);
                cfg.ConfigureEndpoints(context);
            });

            massTransitOptions.AddRider(rider =>
            {
                var settings = configuration.GetSection(EventStreamingSettings.KEY).Get<EventStreamingSettings>()!;

                rider.AddProducer<CreateAvatarRequestEvent>(settings.CreateAvatarRequestsTopicName);
                rider.AddProducer<ProcessPromptRequestEvent>(settings.ProcessPromptRequestsTopicName);
                rider.AddProducer<GenerateImageRequestEvent>(settings.GenerateImageRequestsTopicName);

                rider.AddConsumer<AvatarCreationResultJobConsumer>();
                rider.AddConsumer<PromptProcessingResultJobConsumer>();
                rider.AddConsumer<ImagesGenerationResultJobConsumer>();

                rider.UsingKafka((context, k) =>
                {
                    k.Host(settings.KafkaBootstrapServers, h =>
                    {
                        h.UseSasl(s =>
                        {
                            s.Username = settings.KafkaUsername;
                            s.Password = settings.KafkaPassword;
                            s.Mechanism = Enum.Parse<SaslMechanism>(settings.KafkaSaslMechanism, true);
                            s.SecurityProtocol = Enum.Parse<SecurityProtocol>(settings.KafkaSecurityProtocol, true);
                        });
                    });

                    k.TopicEndpoint<CreateAvatarResponseEvent>(settings.CreateAvatarResponsesTopicName,
                        $"{settings.ConsumerGroupId}-{settings.CreateAvatarResponsesTopicName}",
                        consumerOptions =>
                        {
                            consumerOptions.ConfigureConsumer<AvatarCreationResultJobConsumer>(context);
                            consumerOptions.AutoOffsetReset = AutoOffsetReset.Earliest;
                            consumerOptions.ConcurrentMessageLimit = 8;
                            UseDefaultRetryCircuitBreakerPolicy(consumerOptions);
                        });
                    k.TopicEndpoint<ProcessPromptResponseEvent>(settings.ProcessPromptResponsesTopicName,
                        $"{settings.ConsumerGroupId}-{settings.ProcessPromptResponsesTopicName}",
                        consumerOptions =>
                        {
                            consumerOptions.ConfigureConsumer<PromptProcessingResultJobConsumer>(context);
                            consumerOptions.AutoOffsetReset = AutoOffsetReset.Earliest;
                            consumerOptions.ConcurrentMessageLimit = 8;
                            UseDefaultRetryCircuitBreakerPolicy(consumerOptions);
                        });
                    k.TopicEndpoint<GenerateImageResponseEvent>(settings.GenerateImageResponsesTopicName,
                        $"{settings.ConsumerGroupId}-{settings.GenerateImageResponsesTopicName}",
                        consumerOptions =>
                        {
                            consumerOptions.ConfigureConsumer<ImagesGenerationResultJobConsumer>(context);
                            consumerOptions.AutoOffsetReset = AutoOffsetReset.Earliest;
                            consumerOptions.ConcurrentMessageLimit = 8;
                            UseDefaultRetryCircuitBreakerPolicy(consumerOptions);
                        });
                });
            });
        });
    }

    private static void UseDefaultRetryCircuitBreakerPolicy<T>(IKafkaTopicReceiveEndpointConfigurator<Ignore, T> consumerOptions) where T : class
    {
        consumerOptions.UseMessageRetry(r => r.Intervals(100, 500, 1000, 5000));
        consumerOptions.UseKillSwitch(ksOptions => ksOptions
            .SetActivationThreshold(10)
            .SetTripThreshold(10)
            .SetRestartTimeout(TimeSpan.FromMinutes(1)));
    }
}