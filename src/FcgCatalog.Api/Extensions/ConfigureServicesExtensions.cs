using FcgCatalog.Api.HealthChecks;
using FcgCatalog.Api.HostedServices;
using FcgCatalog.Infrastructure.Database;
using FcgCatalog.Infrastructure.Repositories.Games;
using FcgCatalog.Domain.Repositories.Games;
using FcgCatalog.IoC;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi;

namespace FcgCatalog.Api.Extensions;

public static class ConfigureServicesExtensions
{
    public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication();
        services.AddAuthorization();
        services.AuthnAuthzConfig();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Cole somente o token JWT. O prefixo 'Bearer' será adicionado automaticamente."
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("bearer", document)] = []
            });
        });

        services.ConfigureAppDependencies(configuration);

        services.AddScoped<IGameReviewRepository, GameReviewRepository>();

        services.AddHealthChecks()
            .AddDbContextCheck<FcgCatalogDbContext>()
            .AddMongoDb()
            // Degraded: a indisponibilidade da busca não deve reiniciar o pod nem derrubar o CRUD.
            .AddCheck<OpenSearchHealthCheck>("opensearch", failureStatus: HealthStatus.Degraded);

        services.AddHostedService<SearchIndexInitializer>();
    }
}
