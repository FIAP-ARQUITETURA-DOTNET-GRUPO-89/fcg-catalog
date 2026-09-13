using FcgCatalog.Application.Commands.Reviews;
using FcgCatalog.Application.Handlers.Reviews;
using FcgCatalog.Domain.Entities;
using FcgCatalog.Domain.Repositories.Games;
using FcgCatalog.SharedKernel.Exceptions;
using NSubstitute;
using Shouldly;

namespace FcgCatalog.UnitTests.Application.Handlers.Review;

public class UpdateGameReviewHandlerTests
{
    [Fact]
    public async Task Handle_QuandoAvaliacaoNaoEncontrada_LancaNotFoundException()
    {
        // Arrange
        var reviewRepositoryMock = Substitute.For<IGameReviewRepository>();

        reviewRepositoryMock.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((GameReview?)null);

        var handler = new UpdateGameReviewHandler(reviewRepositoryMock);
        var command = new UpdateGameReviewCommand(5, "Novo comentário")
        {
            Id = Guid.NewGuid()
        };

        // Act
        var action = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await Should.ThrowAsync<NotFoundException>(action);
        await reviewRepositoryMock.DidNotReceive().UpdateAsync(Arg.Any<GameReview>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_QuandoAvaliacaoExistente_AtualizaEPersisteComSucesso()
    {
        // Arrange
        var reviewRepositoryMock = Substitute.For<IGameReviewRepository>();

        var review = new GameReview(Guid.NewGuid(), Guid.NewGuid(), 3, "Comentário antigo");

        reviewRepositoryMock.GetByIdAsync(review.Id, Arg.Any<CancellationToken>())
            .Returns(review);

        var handler = new UpdateGameReviewHandler(reviewRepositoryMock);
        var command = new UpdateGameReviewCommand(5, "Comentário atualizado")
        {
            Id = review.Id
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeTrue();

        review.Nota.ShouldBe(5);
        review.Comentario.ShouldBe("Comentário atualizado");

        await reviewRepositoryMock.Received(1).UpdateAsync(review, Arg.Any<CancellationToken>());
    }
}
