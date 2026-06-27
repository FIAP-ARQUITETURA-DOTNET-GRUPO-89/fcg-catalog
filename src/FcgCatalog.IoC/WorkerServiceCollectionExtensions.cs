using FcgCatalog.Application;
using FcgCatalog.Domain;
using FcgCatalog.Domain.Repositories.Games;
using FcgCatalog.Domain.Repositories.Library;
using FcgCatalog.Domain.Repositories.Orders;
using FcgCatalog.Infrastructure.Database;
using FcgCatalog.Infrastructure.Repositories.Games;
using FcgCatalog.Infrastructure.Repositories.Library;
using FcgCatalog.Infrastructure.Repositories.Orders;
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

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
            typeof(IDomainAssembly).Assembly,
            typeof(IApplicationAssembly).Assembly));

        services.AddDbContext<FcgCatalogDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default"),
                npgsql => npgsql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorCodesToAdd: null)));

        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUserGameRepository, UserGameRepository>();
    }
}
