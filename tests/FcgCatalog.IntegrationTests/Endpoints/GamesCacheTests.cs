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
public class GamesCacheTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task GET_QuandoConsultarDuasVezes_DeveRetornarMesmoResultado()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        var admin = await TestAuthHelper.CreateAdminClientAsync(fixture);

        await admin.PostAsJsonAsync(
            "/api/games",
            new CreateGameCommand(
                "Halo",
                "FPS",
                199.90m,
                new DateTime(
                    2001,
                    11,
                    15,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc),
                ClassificacaoEtaria.Dezesseis),
            TestContext.Current.CancellationToken);

        var customer = await TestAuthHelper.CreateUserCustomerAsync(fixture);

        // Act
        var firstResponse = await customer.GetAsync(
            "/api/games?page=1&pageSize=10",
            TestContext.Current.CancellationToken);

        var firstBody =
            await firstResponse.Content.ReadFromJsonAsync<
                FcgCatalog.SharedKernel.Responses.PagedResponse<GameResponse>>(
                TestContext.Current.CancellationToken);

        var secondResponse = await customer.GetAsync(
            "/api/games?page=1&pageSize=10",
            TestContext.Current.CancellationToken);

        var secondBody =
            await secondResponse.Content.ReadFromJsonAsync<
                FcgCatalog.SharedKernel.Responses.PagedResponse<GameResponse>>(
                TestContext.Current.CancellationToken);

        // Assert
        firstResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        secondResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        firstBody.ShouldNotBeNull();
        secondBody.ShouldNotBeNull();

        firstBody!.TotalCount.ShouldBe(secondBody!.TotalCount);
        firstBody.Items.Count().ShouldBe(secondBody.Items.Count());

        firstBody.Items
            .Select(game => game.Id)
            .ShouldBe(
                secondBody.Items.Select(game => game.Id));

        firstBody.Items
            .Select(game => game.Nome)
            .ShouldBe(
                secondBody.Items.Select(game => game.Nome));

        firstBody.Items
            .ShouldContain(game => game.Nome == "Halo");
    }

    [Fact]
    public async Task GET_AposAlterarPreco_DeveRetornarNovoPreco()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        var admin = await TestAuthHelper.CreateAdminClientAsync(fixture);

        var createdResponse = await admin.PostAsJsonAsync(
            "/api/games",
            new CreateGameCommand(
                "Halo",
                "FPS",
                199.90m,
                new DateTime(
                    2001,
                    11,
                    15,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc),
                ClassificacaoEtaria.Dezesseis),
            TestContext.Current.CancellationToken);

        createdResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var game =
            await createdResponse.Content.ReadFromJsonAsync<GameResponse>(
                TestContext.Current.CancellationToken);

        game.ShouldNotBeNull();

        var customer = await TestAuthHelper.CreateUserCustomerAsync(fixture);

        // Primeiro GET cria o cache da lista.
        var firstResponse = await customer.GetAsync(
            "/api/games?page=1&pageSize=10",
            TestContext.Current.CancellationToken);

        firstResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var firstBody =
            await firstResponse.Content.ReadFromJsonAsync<
                FcgCatalog.SharedKernel.Responses.PagedResponse<GameResponse>>(
                TestContext.Current.CancellationToken);

        firstBody.ShouldNotBeNull();

        firstBody!.Items
            .ShouldContain(item =>
                item.Id == game!.Id &&
                item.Preco == 199.90m);

        // Act
        var updateResponse = await admin.PatchAsJsonAsync(
            $"/api/games/{game!.Id}/price",
            new UpdatePriceCommand(149.90m),
            TestContext.Current.CancellationToken);

        // Assert
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // O cache deve ter sido invalidado pela alteração do preço.
        var secondResponse = await customer.GetAsync(
            "/api/games?page=1&pageSize=10",
            TestContext.Current.CancellationToken);

        secondResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var secondBody =
            await secondResponse.Content.ReadFromJsonAsync<
                FcgCatalog.SharedKernel.Responses.PagedResponse<GameResponse>>(
                TestContext.Current.CancellationToken);

        secondBody.ShouldNotBeNull();

        secondBody!.Items
            .ShouldContain(item =>
                item.Id == game.Id &&
                item.Preco == 149.90m);
    }

    [Fact]
    public async Task GET_AposAtualizarJogo_DeveRetornarDadosAtualizados()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        var admin = await TestAuthHelper.CreateAdminClientAsync(fixture);

        var createdResponse = await admin.PostAsJsonAsync(
            "/api/games",
            new CreateGameCommand(
                "Halo",
                "FPS",
                199.90m,
                new DateTime(
                    2001,
                    11,
                    15,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc),
                ClassificacaoEtaria.Dezesseis),
            TestContext.Current.CancellationToken);

        createdResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var game =
            await createdResponse.Content.ReadFromJsonAsync<GameResponse>(
                TestContext.Current.CancellationToken);

        game.ShouldNotBeNull();

        var customer = await TestAuthHelper.CreateUserCustomerAsync(fixture);

        // Primeiro GET cria o cache da lista.
        var firstResponse = await customer.GetAsync(
            "/api/games?page=1&pageSize=10",
            TestContext.Current.CancellationToken);

        firstResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var firstBody =
            await firstResponse.Content.ReadFromJsonAsync<
                FcgCatalog.SharedKernel.Responses.PagedResponse<GameResponse>>(
                TestContext.Current.CancellationToken);

        firstBody.ShouldNotBeNull();

        firstBody!.Items
            .ShouldContain(item =>
                item.Id == game!.Id &&
                item.Nome == "Halo");

        // Act
        var updateCommand = new UpdateGameCommand(
            "Halo Atualizado",
            "FPS atualizado",
            249.90m,
            new DateTime(
                2001,
                11,
                15,
                0,
                0,
                0,
                DateTimeKind.Utc),
            ClassificacaoEtaria.Dezesseis)
        {
            Id = game.Id
        };

        var updateResponse = await admin.PutAsJsonAsync(
            $"/api/games/{game.Id}",
            updateCommand,
            TestContext.Current.CancellationToken);

        // Assert
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // O cache deve ter sido invalidado após a atualização.
        var secondResponse = await customer.GetAsync(
            "/api/games?page=1&pageSize=10",
            TestContext.Current.CancellationToken);

        secondResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var secondBody =
            await secondResponse.Content.ReadFromJsonAsync<
                FcgCatalog.SharedKernel.Responses.PagedResponse<GameResponse>>(
                TestContext.Current.CancellationToken);

        secondBody.ShouldNotBeNull();

        secondBody!.Items
            .ShouldContain(item =>
                item.Id == game.Id &&
                item.Nome == "Halo Atualizado" &&
                item.Preco == 249.90m);
    }

    [Fact]
    public async Task GET_AposExcluirJogo_NaoDeveRetornarJogoExcluido()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        var admin = await TestAuthHelper.CreateAdminClientAsync(fixture);

        var createdResponse = await admin.PostAsJsonAsync(
            "/api/games",
            new CreateGameCommand(
                "Halo",
                "FPS",
                199.90m,
                new DateTime(
                    2001,
                    11,
                    15,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc),
                ClassificacaoEtaria.Dezesseis),
            TestContext.Current.CancellationToken);

        createdResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var game =
            await createdResponse.Content.ReadFromJsonAsync<GameResponse>(
                TestContext.Current.CancellationToken);

        game.ShouldNotBeNull();

        var customer = await TestAuthHelper.CreateUserCustomerAsync(fixture);

        // Primeiro GET cria o cache da lista.
        var firstResponse = await customer.GetAsync(
            "/api/games?page=1&pageSize=10",
            TestContext.Current.CancellationToken);

        firstResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var firstBody =
            await firstResponse.Content.ReadFromJsonAsync<
                FcgCatalog.SharedKernel.Responses.PagedResponse<GameResponse>>(
                TestContext.Current.CancellationToken);

        firstBody.ShouldNotBeNull();

        firstBody!.Items
            .ShouldContain(item => item.Id == game!.Id);

        // Act
        var deleteResponse = await admin.DeleteAsync(
            $"/api/games/{game.Id}",
            TestContext.Current.CancellationToken);

        // Assert
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // O cache deve ter sido invalidado após a exclusão.
        var secondResponse = await customer.GetAsync(
            "/api/games?page=1&pageSize=10",
            TestContext.Current.CancellationToken);

        secondResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var secondBody =
            await secondResponse.Content.ReadFromJsonAsync<
                FcgCatalog.SharedKernel.Responses.PagedResponse<GameResponse>>(
                TestContext.Current.CancellationToken);

        secondBody.ShouldNotBeNull();

        secondBody!.Items
            .ShouldNotContain(item => item.Id == game.Id);
    }

    [Fact]
    public async Task GET_AposCriarJogo_DeveRetornarNovoJogo()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        var admin = await TestAuthHelper.CreateAdminClientAsync(fixture);
        var customer = await TestAuthHelper.CreateUserCustomerAsync(fixture);

        // Primeiro GET cria o cache da lista.
        var firstResponse = await customer.GetAsync(
            "/api/games?page=1&pageSize=10",
            TestContext.Current.CancellationToken);

        firstResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var firstBody =
            await firstResponse.Content.ReadFromJsonAsync<
                FcgCatalog.SharedKernel.Responses.PagedResponse<GameResponse>>(
                TestContext.Current.CancellationToken);

        firstBody.ShouldNotBeNull();

        var initialCount = firstBody!.TotalCount;

        // Act
        var createResponse = await admin.PostAsJsonAsync(
            "/api/games",
            new CreateGameCommand(
                "Novo Jogo Cache",
                "Jogo criado para testar a invalidação do cache.",
                149.90m,
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

        // Assert
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var createdGame =
            await createResponse.Content.ReadFromJsonAsync<GameResponse>(
                TestContext.Current.CancellationToken);

        createdGame.ShouldNotBeNull();

        // O cache deve ter sido invalidado após a criação.
        var secondResponse = await customer.GetAsync(
            "/api/games?page=1&pageSize=10",
            TestContext.Current.CancellationToken);

        secondResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var secondBody =
            await secondResponse.Content.ReadFromJsonAsync<
                FcgCatalog.SharedKernel.Responses.PagedResponse<GameResponse>>(
                TestContext.Current.CancellationToken);

        secondBody.ShouldNotBeNull();

        secondBody!.TotalCount.ShouldBe(initialCount + 1);

        secondBody.Items
            .ShouldContain(item =>
                item.Id == createdGame!.Id &&
                item.Nome == "Novo Jogo Cache");
    }

    [Fact]
    public async Task GET_PorId_QuandoConsultarDuasVezes_DeveRetornarMesmoResultado()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        var admin = await TestAuthHelper.CreateAdminClientAsync(fixture);

        var createdResponse = await admin.PostAsJsonAsync(
            "/api/games",
            new CreateGameCommand(
                "Halo",
                "FPS",
                199.90m,
                new DateTime(
                    2001,
                    11,
                    15,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc),
                ClassificacaoEtaria.Dezesseis),
            TestContext.Current.CancellationToken);

        createdResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var game =
            await createdResponse.Content.ReadFromJsonAsync<GameResponse>(
                TestContext.Current.CancellationToken);

        game.ShouldNotBeNull();

        var customer = await TestAuthHelper.CreateUserCustomerAsync(fixture);

        // Act
        var firstResponse = await customer.GetAsync(
            $"/api/games/{game!.Id}",
            TestContext.Current.CancellationToken);

        var firstBody =
            await firstResponse.Content.ReadFromJsonAsync<GameResponse>(
                TestContext.Current.CancellationToken);

        var secondResponse = await customer.GetAsync(
            $"/api/games/{game.Id}",
            TestContext.Current.CancellationToken);

        var secondBody =
            await secondResponse.Content.ReadFromJsonAsync<GameResponse>(
                TestContext.Current.CancellationToken);

        // Assert
        firstResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        secondResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        firstBody.ShouldNotBeNull();
        secondBody.ShouldNotBeNull();

        firstBody!.Id.ShouldBe(secondBody!.Id);
        firstBody.Nome.ShouldBe(secondBody.Nome);
        firstBody.Descricao.ShouldBe(secondBody.Descricao);
        firstBody.Preco.ShouldBe(secondBody.Preco);
    }

    [Fact]
    public async Task GET_PorId_AposAlterarPreco_DeveRetornarNovoPreco()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        var admin = await TestAuthHelper.CreateAdminClientAsync(fixture);

        var createdResponse = await admin.PostAsJsonAsync(
            "/api/games",
            new CreateGameCommand(
                "Halo",
                "FPS",
                199.90m,
                new DateTime(
                    2001,
                    11,
                    15,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc),
                ClassificacaoEtaria.Dezesseis),
            TestContext.Current.CancellationToken);

        createdResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var game =
            await createdResponse.Content.ReadFromJsonAsync<GameResponse>(
                TestContext.Current.CancellationToken);

        game.ShouldNotBeNull();

        var customer = await TestAuthHelper.CreateUserCustomerAsync(fixture);

        // Primeiro GET cria o cache do jogo por ID.
        var firstResponse = await customer.GetAsync(
            $"/api/games/{game!.Id}",
            TestContext.Current.CancellationToken);

        firstResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var firstBody =
            await firstResponse.Content.ReadFromJsonAsync<GameResponse>(
                TestContext.Current.CancellationToken);

        firstBody.ShouldNotBeNull();
        firstBody!.Preco.ShouldBe(199.90m);

        // Act
        var updateResponse = await admin.PatchAsJsonAsync(
            $"/api/games/{game.Id}/price",
            new UpdatePriceCommand(149.90m),
            TestContext.Current.CancellationToken);

        // Assert
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // O cache individual deve ter sido invalidado após a alteração.
        var secondResponse = await customer.GetAsync(
            $"/api/games/{game.Id}",
            TestContext.Current.CancellationToken);

        secondResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var secondBody =
            await secondResponse.Content.ReadFromJsonAsync<GameResponse>(
                TestContext.Current.CancellationToken);

        secondBody.ShouldNotBeNull();
        secondBody!.Preco.ShouldBe(149.90m);
    }

    [Fact]
    public async Task GET_PorId_AposAtualizarJogo_DeveRetornarDadosAtualizados()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        var admin = await TestAuthHelper.CreateAdminClientAsync(fixture);

        var createdResponse = await admin.PostAsJsonAsync(
            "/api/games",
            new CreateGameCommand(
                "Halo",
                "FPS",
                199.90m,
                new DateTime(
                    2001,
                    11,
                    15,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc),
                ClassificacaoEtaria.Dezesseis),
            TestContext.Current.CancellationToken);

        createdResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var game =
            await createdResponse.Content.ReadFromJsonAsync<GameResponse>(
                TestContext.Current.CancellationToken);

        game.ShouldNotBeNull();

        var customer = await TestAuthHelper.CreateUserCustomerAsync(fixture);

        // Primeiro GET cria o cache do jogo por ID.
        var firstResponse = await customer.GetAsync(
            $"/api/games/{game!.Id}",
            TestContext.Current.CancellationToken);

        firstResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var firstBody =
            await firstResponse.Content.ReadFromJsonAsync<GameResponse>(
                TestContext.Current.CancellationToken);

        firstBody.ShouldNotBeNull();
        firstBody!.Nome.ShouldBe("Halo");

        // Act
        var updateCommand = new UpdateGameCommand(
            "Halo Atualizado",
            "FPS atualizado",
            249.90m,
            new DateTime(
                2001,
                11,
                15,
                0,
                0,
                0,
                DateTimeKind.Utc),
            ClassificacaoEtaria.Dezesseis)
        {
            Id = game.Id
        };

        var updateResponse = await admin.PutAsJsonAsync(
            $"/api/games/{game.Id}",
            updateCommand,
            TestContext.Current.CancellationToken);

        // Assert
        updateResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // O cache individual deve ter sido invalidado após a atualização.
        var secondResponse = await customer.GetAsync(
            $"/api/games/{game.Id}",
            TestContext.Current.CancellationToken);

        secondResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var secondBody =
            await secondResponse.Content.ReadFromJsonAsync<GameResponse>(
                TestContext.Current.CancellationToken);

        secondBody.ShouldNotBeNull();

        secondBody!.Nome.ShouldBe("Halo Atualizado");
        secondBody.Descricao.ShouldBe("FPS atualizado");
        secondBody.Preco.ShouldBe(249.90m);
    }

    [Fact]
    public async Task GET_PorId_AposExcluirJogo_DeveRetornar404()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();

        var admin = await TestAuthHelper.CreateAdminClientAsync(fixture);

        var createdResponse = await admin.PostAsJsonAsync(
            "/api/games",
            new CreateGameCommand(
                "Halo",
                "FPS",
                199.90m,
                new DateTime(
                    2001,
                    11,
                    15,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc),
                ClassificacaoEtaria.Dezesseis),
            TestContext.Current.CancellationToken);

        createdResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var game =
            await createdResponse.Content.ReadFromJsonAsync<GameResponse>(
                TestContext.Current.CancellationToken);

        game.ShouldNotBeNull();

        var customer = await TestAuthHelper.CreateUserCustomerAsync(fixture);

        // Primeiro GET cria o cache do jogo por ID.
        var firstResponse = await customer.GetAsync(
            $"/api/games/{game!.Id}",
            TestContext.Current.CancellationToken);

        firstResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var firstBody =
            await firstResponse.Content.ReadFromJsonAsync<GameResponse>(
                TestContext.Current.CancellationToken);

        firstBody.ShouldNotBeNull();
        firstBody!.Id.ShouldBe(game.Id);

        // Act
        var deleteResponse = await admin.DeleteAsync(
            $"/api/games/{game.Id}",
            TestContext.Current.CancellationToken);

        // Assert
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // O cache individual deve ter sido invalidado após a exclusão.
        var secondResponse = await customer.GetAsync(
            $"/api/games/{game.Id}",
            TestContext.Current.CancellationToken);

        secondResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
