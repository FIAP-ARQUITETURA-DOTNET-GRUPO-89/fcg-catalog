using FcgCatalog.Application.Commands.Games;
using FcgCatalog.Application.Handlers.Games;
using FcgCatalog.Domain.Entities;
using FcgCatalog.Domain.Enums;
using FcgCatalog.Domain.Repositories.Games;
using FcgCatalog.SharedKernel.Exceptions;
using NSubstitute;
using Shouldly;

namespace FcgCatalog.UnitTests.Application.Handlers.Games;

public class UpdatePriceHandlerTests
{
    [Fact]
    public async Task Handle_QuandoJogoNaoEncontrado_LancaNotFound()
    {
        var repo = Substitute.For<IGameRepository>();
        repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Game?)null);

        var handler = new UpdatePriceHandler(repo);

        await Should.ThrowAsync<NotFoundException>(() =>
            handler.Handle(new UpdatePriceCommand(50m) { Id = Guid.NewGuid() }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_QuandoJogoExistente_AtualizaPrecoEPersiste()
    {
        var repo = Substitute.For<IGameRepository>();
        var game = new Game("Halo", "desc", 100m, DateTime.Today, ClassificacaoEtaria.Livre);
        repo.GetByIdAsync(game.Id, Arg.Any<CancellationToken>()).Returns(game);

        var handler = new UpdatePriceHandler(repo);

        var result = await handler.Handle(new UpdatePriceCommand(80m) { Id = game.Id }, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        game.Preco.ShouldBe(80m);
        await repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
