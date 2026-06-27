using System.Net;
using System.Net.Http.Json;
using FcgCatalog.Application.Commands.Games;
using FcgCatalog.Application.Responses.Games;
using FcgCatalog.Domain.Enums;
using FcgCatalog.IntegrationTests.Fixtures;
using FcgCatalog.IntegrationTests.TestHelpers;
using Shouldly;

namespace FcgCatalog.IntegrationTests.Endpoints;

[Collection("IntegrationTests")]
public class GamesEndpointsTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task POST_QuandoAdmin_DeveCriarJogo()
    {
        await fixture.ResetDatabaseAsync();
        var client = await TestAuthHelper.CreateAdminClientAsync(fixture);

        var command = new CreateGameCommand("Halo", "FPS lendário", 199.90m, new DateTime(2001, 11, 15), ClassificacaoEtaria.Dezesseis);

        var response = await client.PostAsJsonAsync("/api/games", command);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<GameResponse>();
        body.ShouldNotBeNull();
        body!.Nome.ShouldBe("Halo");
    }

    [Fact]
    public async Task POST_QuandoCustomer_DeveRetornar403()
    {
        await fixture.ResetDatabaseAsync();
        var client = await TestAuthHelper.CreateUserCustomerAsync(fixture);

        var command = new CreateGameCommand("Doom", "FPS", 89.90m, new DateTime(1993, 12, 10), ClassificacaoEtaria.Dezesseis);

        var response = await client.PostAsJsonAsync("/api/games", command);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task POST_QuandoAnonimo_DeveRetornar401()
    {
        var client = TestAuthHelper.CreateAnonymousClient(fixture);
        var command = new CreateGameCommand("Mario", "Plataforma", 159.90m, new DateTime(1985, 9, 13), ClassificacaoEtaria.Livre);

        var response = await client.PostAsJsonAsync("/api/games", command);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GET_QuandoAutenticado_DeveRetornarListaPaginada()
    {
        await fixture.ResetDatabaseAsync();
        var admin = await TestAuthHelper.CreateAdminClientAsync(fixture);
        await admin.PostAsJsonAsync("/api/games",
            new CreateGameCommand("Halo", "FPS", 199.90m, new DateTime(2001, 11, 15), ClassificacaoEtaria.Dezesseis));

        var customer = await TestAuthHelper.CreateUserCustomerAsync(fixture);
        var response = await customer.GetAsync("/api/games");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PATCH_Preco_QuandoAdmin_DeveAtualizar()
    {
        await fixture.ResetDatabaseAsync();
        var admin = await TestAuthHelper.CreateAdminClientAsync(fixture);

        var created = await admin.PostAsJsonAsync("/api/games",
            new CreateGameCommand("Tetris", "Puzzle", 19.90m, new DateTime(1984, 6, 6), ClassificacaoEtaria.Livre));
        var game = await created.Content.ReadFromJsonAsync<GameResponse>();

        var response = await admin.PatchAsJsonAsync($"/api/games/{game!.Id}/price", new UpdatePriceCommand(29.90m));

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DELETE_QuandoAdmin_DeveInativar()
    {
        await fixture.ResetDatabaseAsync();
        var admin = await TestAuthHelper.CreateAdminClientAsync(fixture);

        var created = await admin.PostAsJsonAsync("/api/games",
            new CreateGameCommand("Pong", "Clássico", 9.90m, new DateTime(1972, 11, 29), ClassificacaoEtaria.Livre));
        var game = await created.Content.ReadFromJsonAsync<GameResponse>();

        var response = await admin.DeleteAsync($"/api/games/{game!.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }
}
