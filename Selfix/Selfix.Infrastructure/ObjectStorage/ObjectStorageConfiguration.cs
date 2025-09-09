using Amazon.Runtime;
using Amazon.S3;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Selfix.Application.ServicesAbstractions.ObjectStorage;
using Selfix.Shared.Settings;

namespace Selfix.Infrastructure.ObjectStorage;

internal static class ObjectStorageConfiguration
{
    public static IServiceCollection AddObjectStorage(this IServiceCollection services)
    {
        return services
            .AddS3Client()
            .AddScoped<IObjectStorageService, ObjectStorageService>();
    }

    private static IServiceCollection AddS3Client(this IServiceCollection services)
    {
        services.AddSingleton<IAmazonS3>(sp => 
        {
            var settings = sp.GetRequiredService<IOptions<ObjectStorageSettings>>().Value;

            var config = new AmazonS3Config
            {
                AuthenticationRegion = settings.Region,
                ServiceURL = settings.Endpoint,
                ForcePathStyle = true
            };

            return new AmazonS3Client(settings.AccessKey, settings.SecretKey, config);
        });

        return services;
    }
}