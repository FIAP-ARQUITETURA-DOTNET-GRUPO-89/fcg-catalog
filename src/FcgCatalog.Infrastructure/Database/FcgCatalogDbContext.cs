using FcgCatalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FcgCatalog.Infrastructure.Database;

public class FcgCatalogDbContext(DbContextOptions<FcgCatalogDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(FcgCatalogDbContext).Assembly);

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        => configurationBuilder
            .Properties<string>()
            .AreUnicode(false)
            .HaveMaxLength(255);
}
