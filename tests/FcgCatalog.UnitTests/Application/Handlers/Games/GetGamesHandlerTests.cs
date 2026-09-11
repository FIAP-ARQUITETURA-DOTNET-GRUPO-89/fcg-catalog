using FcgCatalog.Application.Handlers.Games;
using FcgCatalog.Application.Interfaces;
using FcgCatalog.Application.Queries.Games;
using FcgCatalog.Application.Responses.Games;
using FcgCatalog.Domain.Entities;
using FcgCatalog.Domain.Repositories.Games;
using FcgCatalog.SharedKernel.Responses;
using NSubstitute;
using Shouldly;

namespace FcgCatalog.UnitTests.Application.Handlers.Games;

public class GetGamesHandlerTests
{
    [Fact]
    public async Task Handle_QuandoCachePossuiDados_RetornaCacheSemConsultarRepositorio()
    {
        // Arrange
        var repo = Substitute.For<IGameRepository>();
        var cache = Substitute.For<IGameCacheService>();

        var cachedResponse = new PagedResponse<GameResponse>(
            [],
            1,
            1,
            10);

        cache.GetGamesAsync(1, 10)
            .Returns(cachedResponse);

        var handler = new GetGamesHandler(
            repo,
            cache);

        var query = new GetGamesQuery(1, 10);

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(cachedResponse);

        // Quando existe cache, o banco não deve ser consultado.
        await repo.DidNotReceive()
            .CountActiveAsync(Arg.Any<CancellationToken>());

        await repo.DidNotReceive()
            .GetPagedAsNoTrackingAsync(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>());

        await cache.DidNotReceive()
            .SetGamesAsync(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<PagedResponse<GameResponse>>());
    }

    [Fact]
    public async Task Handle_QuandoCacheNaoPossuiDados_ConsultaRepositorioESalvaCache()
    {
        // Arrange
        var repo = Substitute.For<IGameRepository>();
        var cache = Substitute.For<IGameCacheService>();

        cache.GetGamesAsync(1, 10)
            .Returns((PagedResponse<GameResponse>?)null);

        var game = new Game(
            "Halo",
            "desc",
            199.90m,
            new DateTime(2020, 1, 1),
            FcgCatalog.Domain.Enums.ClassificacaoEtaria.Dezesseis);

        repo.CountActiveAsync(
            Arg.Any<CancellationToken>())
            .Returns(1);

        repo.GetPagedAsNoTrackingAsync(
            1,
            10,
            Arg.Any<CancellationToken>())
            .Returns([game]);

        var handler = new GetGamesHandler(
            repo,
            cache);

        var query = new GetGamesQuery(1, 10);

        // Act
        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCount.ShouldBe(1);
        result.Value.Items.ShouldHaveSingleItem();
        result.Value.Items.First().Nome.ShouldBe("Halo");

        await repo.Received(1)
            .CountActiveAsync(
                Arg.Any<CancellationToken>());

        await repo.Received(1)
            .GetPagedAsNoTrackingAsync(
                1,
                10,
                Arg.Any<CancellationToken>());

        // Quando ocorre cache miss, o resultado do banco deve ser armazenado.
        await cache.Received(1)
            .SetGamesAsync(
                1,
                10,
                Arg.Any<PagedResponse<GameResponse>>());
    }
}
