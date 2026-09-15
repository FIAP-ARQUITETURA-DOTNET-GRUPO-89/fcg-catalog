using FcgCatalog.Application.Commands.Games;
using FcgCatalog.Application.Handlers.Games;
using FcgCatalog.Application.Interfaces;
using FcgCatalog.Domain.Entities;
using FcgCatalog.Domain.Enums;
using FcgCatalog.Domain.Repositories.Games;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;

namespace FcgCatalog.UnitTests.Application.Handlers.Games;

public class CreateGameHandlerTests
{
    [Fact]
    public async Task Handle_DevePersistirEPublicarResposta()
    {
        // Arrange
        var repo = Substitute.For<IGameRepository>();
        var cache = Substitute.For<IGameCacheService>();

        var handler = new CreateGameHandler(
            repo,
            NullLogger<CreateGameHandler>.Instance,
            cache);

        var command = new CreateGameCommand(
            "Halo",
            "desc",
            199.90m,
            new DateTime(2020, 1, 1),
            ClassificacaoEtaria.Dezesseis);

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Nome.ShouldBe("Halo");

        repo.Received(1).Add(Arg.Any<Game>());
        await repo.Received(1).SaveChangesAsync(
            Arg.Any<CancellationToken>());

        // O cadastro de um jogo deve invalidar o cache da lista.
        await cache.Received(1).InvalidateGameListAsync();
    }
}
