using System;
using FrameUp.ProcessService.Api.Configuration;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;

namespace FrameUp.ProcessService.Api.Extensions;

public static class HealthChecksExtensions
{
    public static WebApplicationBuilder AddCustomHealthChecks(this WebApplicationBuilder builder, Settings settings)
    {
        var factory = new ConnectionFactory
        {
            Uri = new Uri(settings.RabbitMQ.ConnectionString),
            AutomaticRecoveryEnabled = true
        };

        builder.Services
            .AddSingleton(factory)
            .AddHealthChecks()
            .AddNpgSql(settings.PostgreSQL.ConnectionString,
                name: "PostgreSQL",
                failureStatus: HealthStatus.Degraded)
            .AddRabbitMQ(sp => factory.CreateConnectionAsync(), "RabbitMQ",
                HealthStatus.Degraded);


        builder.Services.AddHealthChecksUI(options =>
        {
            options.SetEvaluationTimeInSeconds(120);
            options.MaximumHistoryEntriesPerEndpoint(10);
            options.AddHealthCheckEndpoint("ProcessService", $"{settings.Host}health");
        }).AddInMemoryStorage();

        return builder;
    }

    public static WebApplication UseCustomHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.UseHealthChecksUI(options => { options.UIPath = "/dashboard"; });
        return app;
    }
}