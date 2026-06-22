using FcgCatalog.Application;
using FcgCatalog.Domain;
using FcgCatalog.Domain.Repositories.Orders;
using FcgCatalog.Infrastructure.Database;
using FcgCatalog.Infrastructure.Repositories.Orders;
using FcgCatalog.SharedKernel.Behaviors;
using FcgCatalog.SharedKernel.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FcgCatalog.IoC;

public static class WorkerServiceCollectionExtensions
{
    public static void ConfigureWorkerDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<MassTransitSettings>().Bind(configuration.GetSection("MassTransit"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblies(
                typeof(IDomainEntryPoint).Assembly,
                typeof(IApplicationAssembly).Assembly,
                typeof(ValidationBehavior<,>).Assembly)
        );

        //Banco
        services.AddDbContext<FcgCatalogDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default"),
                npgsql => npgsql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorCodesToAdd: null)));

        // Repositories
        services.AddScoped<IOrderRepository, OrderRepository>();

        // Services
    }
}
