using FrameUp.ProcessService.Application.Contracts;
using FrameUp.ProcessService.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FrameUp.ProcessService.Api.Extensions;

public static class AddFactoriesExtensions
{
    public static IServiceCollection AddFactories(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddScoped<IDateTimeFactory, DefaultDateTimeFactory>();

        return serviceCollection;
    }
}