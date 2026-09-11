using FcgCatalog.Application.Handlers.Games;
using FcgCatalog.Application.Interfaces;
using FcgCatalog.Application.Queries.Games;
using FcgCatalog.Domain.Entities;
using FcgCatalog.Domain.Enums;
using FcgCatalog.Domain.Repositories.Games;
using FcgCatalog.SharedKernel.Exceptions;
using NSubstitute;
using Shouldly;

namespace FcgCatalog.UnitTests.Application.Handlers.Games;

public class GetGameByIdHandlerTests
{
    [Fact]
    public async Task Handle_QuandoCachePossuiDados_RetornaCacheSemConsultarRepositorio()
    {
        // Arrange
        var repo = Substitute.For<IGameRepository>();
        var cache = Substitute.For<IGameCacheService>();

        var gameId = Guid.NewGuid();

        var cachedResponse = new FcgCatalog.Application.Responses.Games.GameResponse(
            gameId,
            "Halo",
            "desc",
            199.90m,
            new DateTime(2020, 1, 1),
            ClassificacaoEtaria.Dezesseis);

        cache.GetGameByIdAsync(gameId)
            .Returns(cachedResponse);

        var handler = new GetGameByIdHandler(
            repo,
            cache);

        var query = new GetGameByIdQuery(gameId);

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(cachedResponse);

        // Quando existe cache, o banco não deve ser consultado.
        await repo.DidNotReceive()
            .GetByIdAsNoTrackingAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());

        await cache.DidNotReceive()
            .SetGameByIdAsync(
                Arg.Any<Guid>(),
                Arg.Any<FcgCatalog.Application.Responses.Games.GameResponse>());
    }

    [Fact]
    public async Task Handle_QuandoCacheNaoPossuiDados_ConsultaRepositorioESalvaCache()
    {
        // Arrange
        var repo = Substitute.For<IGameRepository>();
        var cache = Substitute.For<IGameCacheService>();

        var game = new Game(
            "Halo",
            "desc",
            199.90m,
            new DateTime(2020, 1, 1),
            ClassificacaoEtaria.Dezesseis);

        cache.GetGameByIdAsync(game.Id)
            .Returns((FcgCatalog.Application.Responses.Games.GameResponse?)null);

        repo.GetByIdAsNoTrackingAsync(
            game.Id,
            Arg.Any<CancellationToken>())
            .Returns(game);

        var handler = new GetGameByIdHandler(
            repo,
            cache);

        var query = new GetGameByIdQuery(game.Id);

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Nome.ShouldBe("Halo");

        await repo.Received(1)
            .GetByIdAsNoTrackingAsync(
                game.Id,
                Arg.Any<CancellationToken>());

        // Quando ocorre cache miss, o resultado do banco deve ser armazenado.
        await cache.Received(1)
            .SetGameByIdAsync(
                game.Id,
                Arg.Any<FcgCatalog.Application.Responses.Games.GameResponse>());
    }

    [Fact]
    public async Task Handle_QuandoCacheNaoPossuiDadosEJogoNaoExiste_LancaNotFound()
    {
        // Arrange
        var repo = Substitute.For<IGameRepository>();
        var cache = Substitute.For<IGameCacheService>();

        var gameId = Guid.NewGuid();

        cache.GetGameByIdAsync(gameId)
            .Returns((FcgCatalog.Application.Responses.Games.GameResponse?)null);

        repo.GetByIdAsNoTrackingAsync(
            gameId,
            Arg.Any<CancellationToken>())
            .Returns((Game?)null);

        var handler = new GetGameByIdHandler(
            repo,
            cache);

        var query = new GetGameByIdQuery(gameId);

        // Act
        var action = () => handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        await Should.ThrowAsync<NotFoundException>(action);

        // Jogo inexistente não deve ser armazenado no cache.
        await cache.DidNotReceive()
            .SetGameByIdAsync(
                Arg.Any<Guid>(),
                Arg.Any<FcgCatalog.Application.Responses.Games.GameResponse>());
    }
}
