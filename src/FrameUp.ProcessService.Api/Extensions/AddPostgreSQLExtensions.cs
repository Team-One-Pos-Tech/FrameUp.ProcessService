using FrameUp.ProcessService.Api.Configuration;
using FrameUp.ProcessService.Infra.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FrameUp.ProcessService.Api.Extensions;

public static class AddPostgreSQLExtensions
{
    public static IServiceCollection AddDatabaseContext(
        this IServiceCollection serviceCollection,
        Settings settings
    )
    {
        serviceCollection
            .AddDbContext<ProcessServiceDbContext>(options =>
                options.UseNpgsql(settings.PostgreSQL.ConnectionString));

        return serviceCollection;
    }
}