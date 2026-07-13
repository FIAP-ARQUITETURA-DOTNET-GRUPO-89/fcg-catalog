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
        // Arrange
        var repo = Substitute.For<IGameRepository>();

        repo.GetByIdAsync(
            Arg.Any<Guid>(),
            Arg.Any<CancellationToken>())
            .Returns((Game?)null);

        var handler = new UpdatePriceHandler(repo);

        var command = new UpdatePriceCommand(50m)
        {
            Id = Guid.NewGuid()
        };

        // Act
        var action = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await Should.ThrowAsync<NotFoundException>(action);
    }

    [Fact]
    public async Task Handle_QuandoJogoExistente_AtualizaPrecoEPersiste()
    {
        // Arrange
        var repo = Substitute.For<IGameRepository>();

        var game = new Game(
            "Halo",
            "desc",
            100m,
            DateTime.Today,
            ClassificacaoEtaria.Livre);

        repo.GetByIdAsync(game.Id, Arg.Any<CancellationToken>())
            .Returns(game);

        var handler = new UpdatePriceHandler(repo);

        var command = new UpdatePriceCommand(80m)
        {
            Id = game.Id
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        game.Preco.ShouldBe(80m);

        await repo.Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
