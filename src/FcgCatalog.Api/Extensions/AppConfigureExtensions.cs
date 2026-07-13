using FcgCatalog.Api.Endpoints;
using FcgCatalog.Api.Middlewares;
using FcgCatalog.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace FcgCatalog.Api.Extensions;

public static class AppConfigureExtensions
{
    public static async Task Configure(this WebApplication app)
    {
        app.UseMiddleware<ExceptionMiddleware>();
        app.UseHttpsRedirection();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
                c.RoutePrefix = "";
            });
        }

        var runMigration = app.Environment.IsDevelopment() ||
                           (app.Environment.IsProduction() && Environment.GetEnvironmentVariable("RUN_MIGRATION") == "true");

        if (runMigration)
        {
            using var scope = app.Services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<FcgCatalogDbContext>();
            if (db.Database.IsRelational())
            {
                db.Database.Migrate();
            }
        }

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGamesEndpoints();
        app.MapUserLibraryEndpoints();

        app.MapHealthChecks("/health");
        app.MapHealthChecks("/ready");
    }
}
