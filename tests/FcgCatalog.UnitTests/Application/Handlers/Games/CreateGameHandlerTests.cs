using FcgCatalog.Application.Commands.Games;
using FcgCatalog.Application.Handlers.Games;
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
        var repo = Substitute.For<IGameRepository>();
        var handler = new CreateGameHandler(repo, NullLogger<CreateGameHandler>.Instance);

        var command = new CreateGameCommand("Halo", "desc", 199.90m, new DateTime(2020, 1, 1), ClassificacaoEtaria.Dezesseis);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Nome.ShouldBe("Halo");
        repo.Received(1).Add(Arg.Any<Game>());
        await repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
