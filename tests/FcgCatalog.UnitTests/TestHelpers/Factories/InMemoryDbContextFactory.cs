using FcgCatalog.Infrastructure.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FcgCatalog.UnitTests.TestHelpers.Factories;

public static class InMemoryDbContextFactory
{
    /// <summary>
    /// Cria um DbContext com uma nova conexão SQLite em memória isolada.
    /// </summary>
    public static FcgCatalogDbContext CreateContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        return CreateContext(connection);
    }

    /// <summary>
    /// Cria um DbContext reaproveitando uma conexão SQLite existente (essencial para isolar asserts sem limpar a memória RAM).
    /// </summary>
    public static FcgCatalogDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<FcgCatalogDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new FcgCatalogDbContext(options);

        context.Database.EnsureCreated();

        return context;
    }
}
