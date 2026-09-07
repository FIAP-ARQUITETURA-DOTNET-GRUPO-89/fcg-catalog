using FcgCatalog.Application.Commands.Reviews;
using FcgCatalog.Application.Handlers.Reviews;
using FcgCatalog.Domain.Entities;
using FcgCatalog.Domain.Enums;
using FcgCatalog.Domain.Repositories.Games;
using FcgCatalog.SharedKernel.Exceptions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace FcgCatalog.UnitTests.Application.Handlers.Review;

public class CreateGameReviewHandlerTests
{
    [Fact]
    public async Task Handle_QuandoJogoNaoEncontrado_LancaNotFoundException()
    {
        // Arrange
        var reviewRepositoryMock = Substitute.For<IGameReviewRepository>();
        var gameRepositoryMock = Substitute.For<IGameRepository>();
        var loggerMock = Substitute.For<ILogger<CreateGameReviewHandler>>();

        gameRepositoryMock.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Game?)null);

        var handler = new CreateGameReviewHandler(reviewRepositoryMock, gameRepositoryMock, loggerMock);

        var command = new CreateGameReviewCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            5,
            "Excelente jogo!");

        // Act
        var action = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await Should.ThrowAsync<NotFoundException>(action);
        await reviewRepositoryMock.DidNotReceive().AddAsync(Arg.Any<GameReview>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_QuandoJogoExistente_DevePersistirReviewComSucesso()
    {
        // Arrange
        var reviewRepositoryMock = Substitute.For<IGameReviewRepository>();
        var gameRepositoryMock = Substitute.For<IGameRepository>();
        var loggerMock = Substitute.For<ILogger<CreateGameReviewHandler>>();

        var game = new Game(
            "Halo",
            "desc",
            199.90m,
            new DateTime(2020, 1, 1),
            ClassificacaoEtaria.Dezesseis);

        gameRepositoryMock.GetByIdAsync(game.Id, Arg.Any<CancellationToken>())
            .Returns(game);

        var handler = new CreateGameReviewHandler(reviewRepositoryMock, gameRepositoryMock, loggerMock);

        var command = new CreateGameReviewCommand(
            game.Id,
            Guid.NewGuid(),
            5,
            "Excelente jogo, recomendo muito!");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Nota.ShouldBe(5);
        result.Value.Comentario.ShouldBe("Excelente jogo, recomendo muito!");

        await reviewRepositoryMock.Received(1).AddAsync(Arg.Any<GameReview>(), Arg.Any<CancellationToken>());
    }
}
