using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Sinks.LogBee;
using Serilog.Sinks.LogBee.AspNetCore;

namespace FrameUp.ProcessService.Api.Extensions;

public static class AddLogBeeExtensions
{
    public static WebApplicationBuilder AddLogBee(
        this WebApplicationBuilder builder
    )
    {
        builder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
            .WriteTo
            .LogBee(new LogBeeApiKey(
                    builder.Configuration["LogBee.OrganizationId"]!,
                    builder.Configuration["LogBee.ApplicationId"]!,
                    builder.Configuration["LogBee.ApiUrl"]!),
                services
            )
            .WriteTo.Console());
        return builder;
    }

    public static void UseLogBee(this WebApplication app)
    {
        app.UseLogBeeMiddleware();
    }
}