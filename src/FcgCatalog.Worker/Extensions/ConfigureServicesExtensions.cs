using FcgCatalog.IoC;
using FcgCatalog.Infrastructure.Messaging;
using FcgCatalog.Worker.Consumers;

namespace FcgCatalog.Worker.Extensions;

public static class ConfigureServicesExtensions
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureWorkerDependencies(configuration);

        services.AddMassTransitRabbitMq(configuration, x =>
        {
            x.AddConsumer<OrderPlacedConsumer>();
        });

        return services;
    }
}
