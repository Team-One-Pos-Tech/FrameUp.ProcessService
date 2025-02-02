using System;
using FrameUp.ProcessService.Api.Configuration;
using FrameUp.ProcessService.Application.EventConsumers;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace FrameUp.ProcessService.Api.Extensions;

public static class MassTransitExtensions
{
    public static IServiceCollection AddMassTransit(this IServiceCollection serviceCollection, Settings settings)
    {
        serviceCollection.AddMassTransit(busConfigurator =>
        {
            busConfigurator.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("frameup-process-service"));

            busConfigurator.AddConsumer<VideoReadyToProcessConsumer>();

            busConfigurator.SetKebabCaseEndpointNameFormatter();
            busConfigurator.UsingRabbitMq((context, configurator) =>
            {
                configurator.Host(new Uri(settings.RabbitMQ.ConnectionString));

                configurator.AutoDelete = true;
                configurator.ConfigureEndpoints(context);
            });
        });

        return serviceCollection;
    }
}