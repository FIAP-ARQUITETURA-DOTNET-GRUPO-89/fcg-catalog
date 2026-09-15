using FluentValidation;
using MediatR;
using FcgCatalog.Application;
using FcgCatalog.Domain;
using FcgCatalog.Domain.Repositories.Games;
using FcgCatalog.Domain.Repositories.Library;
using FcgCatalog.Domain.Repositories.Orders;
using FcgCatalog.Infrastructure.Database;
using FcgCatalog.Infrastructure.Messaging;
using FcgCatalog.Infrastructure.Repositories.Games;
using FcgCatalog.Infrastructure.Repositories.Library;
using FcgCatalog.Infrastructure.Repositories.Orders;
using FcgCatalog.SharedKernel.Behaviors;
using FcgCatalog.SharedKernel.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FcgCatalog.Application.Interfaces;
using FcgCatalog.Infrastructure.Caching;

namespace FcgCatalog.IoC;

public static class AppServiceCollectionExtensions
{
    public static void ConfigureAppDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtSettings>().Bind(configuration.GetSection("JwtSettings"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
            typeof(IDomainAssembly).Assembly,
            typeof(IApplicationAssembly).Assembly));

        services.AddValidatorsFromAssemblyContaining<IApplicationAssembly>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddDbContext<FcgCatalogDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default"),
                npgsql => npgsql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorCodesToAdd: null)));

        services.AddMassTransitRabbitMqPublisher(configuration);

        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUserGameRepository, UserGameRepository>();
        services.AddScoped<IGameCacheService, RedisGameCacheService>();
    }
}
