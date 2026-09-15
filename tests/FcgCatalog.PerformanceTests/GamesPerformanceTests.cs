using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using FcgCatalog.Application.Commands.Games;
using FcgCatalog.Application.Responses.Games;
using FcgCatalog.Domain.Enums;
using FcgCatalog.IntegrationTests.Fixtures;
using FcgCatalog.IntegrationTests.TestHelpers;
using Shouldly;

namespace FcgCatalog.PerformanceTests;

public class GamesPerformanceTests : IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture = new();
    private readonly ITestOutputHelper _output;

    public GamesPerformanceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public async ValueTask InitializeAsync()
    {
        await _fixture.InitializeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }

    [Fact]
    public async Task GET_CompararCacheMissECacheHit_DeveMedirPerformance()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();

        var admin =
            await TestAuthHelper.CreateAdminClientAsync(_fixture);

        var createResponse = await admin.PostAsJsonAsync(
            "/api/games",
            new CreateGameCommand(
                "Performance Test Game",
                "Jogo criado para teste de performance do cache.",
                199.90m,
                new DateTime(
                    2024,
                    1,
                    1,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc),
                ClassificacaoEtaria.Livre),
            TestContext.Current.CancellationToken);

        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var customer =
            await TestAuthHelper.CreateUserCustomerAsync(_fixture);

        // Usa a primeira página para consultar dados reais do banco.
        var cacheUrl = "/api/games?page=1&pageSize=10";

        // Act
        // Primeira consulta: cache miss, buscando os dados no banco.
        var missStopwatch = Stopwatch.StartNew();

        var missResponse = await customer.GetAsync(
            cacheUrl,
            TestContext.Current.CancellationToken);

        missStopwatch.Stop();

        missResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var missBody =
            await missResponse.Content.ReadFromJsonAsync<
                FcgCatalog.SharedKernel.Responses.PagedResponse<GameResponse>>(
                TestContext.Current.CancellationToken);

        missBody.ShouldNotBeNull();

        // Consultas seguintes: cache hit, buscando os dados no Redis.
        const int hitRequests = 20;

        var hitStopwatch = Stopwatch.StartNew();

        for (var i = 0; i < hitRequests; i++)
        {
            var hitResponse = await customer.GetAsync(
                cacheUrl,
                TestContext.Current.CancellationToken);

            hitResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

            var hitBody =
                await hitResponse.Content.ReadFromJsonAsync<
                    FcgCatalog.SharedKernel.Responses.PagedResponse<GameResponse>>(
                    TestContext.Current.CancellationToken);

            hitBody.ShouldNotBeNull();

            // Confirma que o cache continua retornando dados válidos.
            hitBody.Items.ShouldNotBeEmpty();
        }

        hitStopwatch.Stop();

        // Calcula a média das requisições atendidas pelo cache.
        var averageHitTime =
            hitStopwatch.Elapsed.TotalMilliseconds / hitRequests;

        // Calcula a redução percentual do tempo médio do cache
        // em comparação com a primeira consulta.
        var performanceImprovement =
            missStopwatch.Elapsed.TotalMilliseconds > 0
                ? (1 -
                    (averageHitTime /
                     missStopwatch.Elapsed.TotalMilliseconds)) * 100
                : 0;

        // Assert
        missStopwatch.ElapsedMilliseconds.ShouldBeGreaterThanOrEqualTo(0);
        averageHitTime.ShouldBeGreaterThanOrEqualTo(0);

        _output.WriteLine(
            $"Cache MISS: {missStopwatch.Elapsed.TotalMilliseconds:F2} ms");

        _output.WriteLine(
            $"Cache HIT ({hitRequests} requisições): {hitStopwatch.Elapsed.TotalMilliseconds:F2} ms total");

        _output.WriteLine(
            $"Cache HIT: {averageHitTime:F2} ms por requisição");

        _output.WriteLine(
            $"Redução média do tempo com cache: {performanceImprovement:F2}%");

        // O teste não exige que o cache seja sempre mais rápido,
        // pois a performance pode variar conforme o ambiente.
        // Os tempos são registrados para análise da performance.
    }
}
