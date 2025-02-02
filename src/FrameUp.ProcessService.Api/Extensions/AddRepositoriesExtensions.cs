using FrameUp.ProcessService.Application.Contracts;
using FrameUp.ProcessService.Infra.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace FrameUp.ProcessService.Api.Extensions;

public static class AddRepositoriesExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddScoped<IFileBucketRepository, MinioBucketRepository>();
        
        return serviceCollection;
    }
}