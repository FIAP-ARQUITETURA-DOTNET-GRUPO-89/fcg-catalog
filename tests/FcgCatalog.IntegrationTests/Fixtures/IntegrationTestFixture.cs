using Aspire.Hosting;
using Aspire.Hosting.Testing;
using FcgCatalog.Infrastructure.Database;
using FcgCatalog.IntegrationTests.TestHelpers;
using FcgCatalog.SharedKernel.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver; // Adicione este using

namespace FcgCatalog.IntegrationTests.Fixtures;

/// <summary>
/// Fixture base para testes de integração da aplicação.
/// Essa classe atua como ponto central de orquestração da infraestrutura de testes.
/// </summary>
public class IntegrationTestFixture : IAsyncLifetime
{
    public DistributedApplication App { get; private set; } = default!;

    private TestDatabaseManager _dbManager = default!;
    private string _connectionString = string.Empty;
    private string _mongoConnectionString = string.Empty; // Armazena a connection string do mongo

    /// <summary>
    /// Inicializa o ambiente de testes.
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Testing");

        var builder = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.FcgCatalog_AppHost>();

        builder.Services.AddHttpContextAccessor();

        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
        builder.Services.AddSingleton<JwtTestTokenGenerator>();

        builder.Services.ConfigureHttpClientDefaults(client =>
        {
            client.ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });
        });

        App = await builder.BuildAsync();
        await App.StartAsync();

        _connectionString = await App.GetConnectionStringAsync("Default") ?? throw new InvalidOperationException("Connection string do Postgres não encontrada");

        // Pega a connection string do MongoDB configurada no Aspire AppHost (ajuste o nome se necessário, ex: "mongodb" ou "MongoDb")
        _mongoConnectionString = await App.GetConnectionStringAsync("mongodb")
                                 ?? await App.GetConnectionStringAsync("MongoDb")
                                 ?? "mongodb://localhost:27017";

        _dbManager = new TestDatabaseManager(_connectionString);
        await _dbManager.InitializeAsync();
        await _dbManager.ResetAsync();
    }

    /// <summary>
    /// Finaliza a execução da aplicação após os testes.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (App is not null)
        {
            await App.StopAsync();
            await App.DisposeAsync();
        }
    }

    /// <summary>
    /// Reseta o banco de dados relacional e o NoSQL para um estado limpo.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        await _dbManager.ResetAsync();

        // Limpa a collection de reviews no MongoDB para evitar poluição entre testes
        if (!string.IsNullOrEmpty(_mongoConnectionString))
        {
            var mongoUrl = MongoUrl.Create(_mongoConnectionString);
            var mongoClient = new MongoClient(mongoUrl);
            var database = mongoClient.GetDatabase(mongoUrl.DatabaseName ?? "fcg_catalog_db");

            await database.DropCollectionAsync("game_reviews");
        }
    }

    /// <summary>
    /// Cria um HttpClient configurado para comunicação com a API.
    /// </summary>
    public HttpClient CreateClient()
        => App.CreateHttpClient("fcgcatalog-api", endpointName: "https");

    /// <summary>
    /// Executa uma função isolada utilizando um <see cref="FcgCatalogDbContext"/> apontando para o banco de testes.
    /// </summary>
    public async Task<T> ExecuteDbContextAsync<T>(Func<FcgCatalogDbContext, Task<T>> action)
    {
        var options = new DbContextOptionsBuilder<FcgCatalogDbContext>()
            .UseNpgsql(_connectionString)
            .Options;

        await using var context = new FcgCatalogDbContext(options);
        return await action(context);
    }
}
