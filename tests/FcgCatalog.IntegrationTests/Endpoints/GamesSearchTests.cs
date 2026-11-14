using System.Net;
using System.Net.Http.Json;
using FcgCatalog.Application.Commands.Games;
using FcgCatalog.Application.Responses.Games;
using FcgCatalog.Domain.Enums;
using FcgCatalog.IntegrationTests.Fixtures;
using FcgCatalog.IntegrationTests.TestHelpers;
using FcgCatalog.SharedKernel.Responses;
using Shouldly;

namespace FcgCatalog.IntegrationTests.Endpoints;

[Collection("IntegrationTests")]
public class GamesSearchTests(IntegrationTestFixture fixture)
{
    private static readonly TimeSpan IndexTimeout = TimeSpan.FromSeconds(15);

    [Fact]
    public async Task Search_ComErroDeDigitacao_DeveEncontrarJogo()
    {
        // Arrange
        var admin = await ResetAsync();

        var zelda = await CreateGameAsync(admin, "The Legend of Zelda", "Aventura em mundo aberto");
        await CreateGameAsync(admin, "Halo", "Tiro em primeira pessoa");

        var customer = await TestAuthHelper.CreateUserCustomerAsync(fixture);

        // Act
        var body = await SearchUntilAsync(customer, "zeldda", r => r?.Items.Any() == true);

        // Assert
        body.ShouldNotBeNull();
        body.Items.First().Id.ShouldBe(zelda.Id);
        body.Items.ShouldNotContain(g => g.Nome == "Halo");
    }

    [Fact]
    public async Task Search_DeveOrdenarPorRelevancia()
    {
        // Arrange
        var admin = await ResetAsync();

        await CreateGameAsync(admin, "Kart Racing Pro", "Simulador de corrida de kart");
        var marioKart = await CreateGameAsync(admin, "Mario Kart", "Corrida com personagens da Nintendo");
        await CreateGameAsync(admin, "Super Mario Odyssey", "Plataforma 3D");

        var customer = await TestAuthHelper.CreateUserCustomerAsync(fixture);

        // Act
        var body = await SearchUntilAsync(customer, "mario kart", r => r?.TotalCount == 3);

        // Assert
        body.ShouldNotBeNull();
        body.Items.First().Id.ShouldBe(marioKart.Id);
        body.Items.Select(g => g.Score).ShouldBeInOrder(SortDirection.Descending);
    }

    [Fact]
    public async Task Search_AposInativarJogo_NaoDeveRetornarJogo()
    {
        // Arrange
        var admin = await ResetAsync();

        var game = await CreateGameAsync(admin, "Minecraft", "Construção e sobrevivência");

        var customer = await TestAuthHelper.CreateUserCustomerAsync(fixture);

        (await SearchUntilAsync(customer, "minecrat", r => r?.Items.Any() == true))!
            .Items.ShouldContain(g => g.Id == game.Id);

        // Act
        var deleteResponse = await admin.DeleteAsync(
            $"/api/games/{game.Id}",
            TestContext.Current.CancellationToken);

        // Assert
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var body = await SearchUntilAsync(customer, "minecrat", r => r?.Items.Any() == false);

        body.ShouldNotBeNull();
        body.Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task Search_SemTermo_DeveRetornarBadRequest()
    {
        var customer = await TestAuthHelper.CreateUserCustomerAsync(fixture);

        var response = await customer.GetAsync(
            "/api/games/search?q=",
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_AlemDoLimiteDePaginacao_DeveRetornarBadRequest()
    {
        var customer = await TestAuthHelper.CreateUserCustomerAsync(fixture);

        var response = await customer.GetAsync(
            "/api/games/search?q=zelda&page=1001&pageSize=10",
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Limpa o banco relacional e reconstrói o índice de busca (vazio) a partir dele, publicando-o no alias.
    /// </summary>
    private async Task<HttpClient> ResetAsync()
    {
        await fixture.ResetDatabaseAsync();

        var admin = await TestAuthHelper.CreateAdminClientAsync(fixture);

        var reindex = await admin.PostAsync(
            "/api/games/search/reindex",
            null,
            TestContext.Current.CancellationToken);

        reindex.StatusCode.ShouldBe(HttpStatusCode.OK);

        return admin;
    }

    private static async Task<GameResponse> CreateGameAsync(HttpClient admin, string nome, string descricao)
    {
        var response = await admin.PostAsJsonAsync(
            "/api/games",
            new CreateGameCommand(
                nome,
                descricao,
                99.90m,
                new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                ClassificacaoEtaria.Livre),
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        return (await response.Content.ReadFromJsonAsync<GameResponse>(
            TestContext.Current.CancellationToken))!;
    }

    // O índice é atualizado de forma quase em tempo real (refresh de ~1s).
    private static Task<PagedResponse<GameSearchResponse>?> SearchUntilAsync(
        HttpClient client,
        string termo,
        Func<PagedResponse<GameSearchResponse>?, bool> predicate)
        => WaitUntil.ForAsync(
            async () =>
            {
                var response = await client.GetAsync(
                    $"/api/games/search?q={Uri.EscapeDataString(termo)}&page=1&pageSize=10",
                    TestContext.Current.CancellationToken);

                response.StatusCode.ShouldBe(HttpStatusCode.OK);

                return await response.Content.ReadFromJsonAsync<PagedResponse<GameSearchResponse>>(
                    TestContext.Current.CancellationToken);
            },
            predicate,
            IndexTimeout);
}
