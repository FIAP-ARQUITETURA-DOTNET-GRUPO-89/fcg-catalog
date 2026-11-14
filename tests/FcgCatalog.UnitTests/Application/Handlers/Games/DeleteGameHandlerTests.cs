using FcgCatalog.Application.Commands.Games;
using FcgCatalog.Application.Handlers.Games;
using FcgCatalog.Application.Interfaces;
using FcgCatalog.Domain.Entities;
using FcgCatalog.Domain.Enums;
using FcgCatalog.Domain.Repositories.Games;
using NSubstitute;
using Shouldly;

namespace FcgCatalog.UnitTests.Application.Handlers.Games;

public class DeleteGameHandlerTests
{
    [Fact]
    public async Task Handle_DeveInativarJogoESincronizarIndice()
    {
        // Arrange
        var repo = Substitute.For<IGameRepository>();
        var cache = Substitute.For<IGameCacheService>();
        var search = Substitute.For<IGameSearchService>();

        var game = new Game("Halo", "FPS", 100m, DateTime.Today, ClassificacaoEtaria.Dezesseis);

        repo.GetByIdAsync(game.Id, Arg.Any<CancellationToken>())
            .Returns(game);

        var handler = new DeleteGameHandler(repo, cache, search);

        // Act
        var result = await handler.Handle(
            new DeleteGameCommand(game.Id),
            CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        game.Inativo.ShouldBeTrue();

        await search.Received(1)
            .IndexAsync(Arg.Is<Game>(g => g.Inativo), Arg.Any<CancellationToken>());
    }
}
