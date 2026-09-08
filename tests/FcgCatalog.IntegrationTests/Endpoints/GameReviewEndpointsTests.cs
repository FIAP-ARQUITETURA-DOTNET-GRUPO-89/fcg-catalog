using System.Net;
using System.Net.Http.Json;
using FcgCatalog.Application.Commands.Reviews;
using FcgCatalog.Application.Responses.Reviews;
using FcgCatalog.Domain.Enums;
using FcgCatalog.IntegrationTests.Fixtures;
using FcgCatalog.IntegrationTests.TestHelpers;
using Shouldly;

namespace FcgCatalog.IntegrationTests.Endpoints;

[Collection("IntegrationTests")]
public class GameReviewEndpointsTests(IntegrationTestFixture fixture)
{
    private async Task<Guid> CriarJogoAuxiliarAsync()
    {
        var gameId = Guid.NewGuid();

        await fixture.ExecuteDbContextAsync(async context =>
        {
            var game = new FcgCatalog.Domain.Entities.Game(
                nome: "Halo",
                descricao: "FPS lendário",
                preco: 199.90m,
                dataLancamento: new DateTime(2001, 11, 15, 0, 0, 0, DateTimeKind.Utc),
                classificacaoEtaria: ClassificacaoEtaria.Dezesseis
            );

            context.Games.Add(game);
            await context.SaveChangesAsync();

            gameId = game.Id;
            return true;
        });

        return gameId;
    }

    [Fact]
    public async Task POST_Review_QuandoCustomer_DeveCriarAvaliacao()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();
        var customerClient = await TestAuthHelper.CreateUserCustomerAsync(fixture);

        var gameId = await CriarJogoAuxiliarAsync();

        var command = new CreateGameReviewCommand(
            gameId,
            Guid.Empty,
            5,
            "Excelente jogo, recomendo muito!");

        // Act
        var response = await customerClient.PostAsJsonAsync($"/api/games/{gameId}/reviews", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<GameReviewResponse>();
        body.ShouldNotBeNull();
        body!.Nota.ShouldBe(5);
        body.Comentario.ShouldBe("Excelente jogo, recomendo muito!");
    }

    [Fact]
    public async Task POST_Review_QuandoAnonimo_DeveRetornar401()
    {
        // Arrange
        var client = TestAuthHelper.CreateAnonymousClient(fixture);
        var jogoId = Guid.NewGuid();

        var command = new CreateGameReviewCommand(
            jogoId,
            Guid.Empty,
            4,
            "Bom jogo");

        // Act
        var response = await client.PostAsJsonAsync($"/api/games/{jogoId}/reviews", command);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GET_Reviews_QuandoCustomer_DeveRetornarListaDeAvaliacoes()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();
        var customerClient = await TestAuthHelper.CreateUserCustomerAsync(fixture);

        var gameId = await CriarJogoAuxiliarAsync();

        var command = new CreateGameReviewCommand(
            gameId,
            Guid.Empty,
            5,
            "Incrível!");

        await customerClient.PostAsJsonAsync($"/api/games/{gameId}/reviews", command);

        // Act
        var response = await customerClient.GetAsync($"/api/games/{gameId}/reviews");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<IReadOnlyList<GameReviewResponse>>();
        body.ShouldNotBeNull();
        body.Count.ShouldBe(1);
        body[0].Comentario.ShouldBe("Incrível!");
    }

    [Fact]
    public async Task PUT_Review_QuandoCustomer_DeveAtualizarAvaliacao()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();
        var customerClient = await TestAuthHelper.CreateUserCustomerAsync(fixture);

        var gameId = await CriarJogoAuxiliarAsync();

        var createCommand = new CreateGameReviewCommand(
            gameId,
            Guid.Empty,
            3,
            "Razoável");

        var createResponse = await customerClient.PostAsJsonAsync($"/api/games/{gameId}/reviews", createCommand);
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var createdReview = await createResponse.Content.ReadFromJsonAsync<GameReviewResponse>();

        var updateCommand = new UpdateGameReviewCommand(5, "Atualizado: Excelente!");

        // Act
        var response = await customerClient.PutAsJsonAsync($"/api/games/reviews/{createdReview!.Id}", updateCommand);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DELETE_Review_QuandoCustomer_DeveRemoverAvaliacao()
    {
        // Arrange
        await fixture.ResetDatabaseAsync();
        var customerClient = await TestAuthHelper.CreateUserCustomerAsync(fixture);

        var gameId = await CriarJogoAuxiliarAsync();

        var createCommand = new CreateGameReviewCommand(
            gameId,
            Guid.Empty,
            1,
            "Não gostei.");

        var createResponse = await customerClient.PostAsJsonAsync($"/api/games/{gameId}/reviews", createCommand);
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var createdReview = await createResponse.Content.ReadFromJsonAsync<GameReviewResponse>();

        // Act
        var response = await customerClient.DeleteAsync($"/api/games/reviews/{createdReview!.Id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }
}
