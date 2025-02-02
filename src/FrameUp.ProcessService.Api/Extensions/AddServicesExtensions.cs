using FrameUp.ProcessService.Application.Contracts;
using FrameUp.ProcessService.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FrameUp.ProcessService.Api.Extensions;

public static class AddServicesExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddScoped<IThumbnailService, ThumbnailService>()
            .AddScoped<IZipFileService, ZipFileService>();

        return serviceCollection;
    }
}