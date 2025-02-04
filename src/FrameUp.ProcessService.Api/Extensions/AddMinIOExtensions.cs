using FrameUp.ProcessService.Api.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;

namespace FrameUp.ProcessService.Api.Extensions;

public static class AddMinIOExtensions
{

    public static IServiceCollection AddMinIO(this IServiceCollection serviceCollection, Settings settings)
    {
        serviceCollection.AddMinio(configureClient => configureClient
            .WithEndpoint(settings.MinIO.Endpoint)
            .WithCredentials(settings.MinIO.AccessKey, settings.MinIO.SecretKey)
            .WithSSL(false)
            .Build());

        return serviceCollection;
    }
}