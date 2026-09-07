using FcgCatalog.Application.Commands.Reviews;
using FcgCatalog.Application.Handlers.Reviews;
using FcgCatalog.Domain.Entities;
using FcgCatalog.Domain.Repositories.Games;
using FcgCatalog.SharedKernel.Exceptions;
using NSubstitute;
using Shouldly;

namespace FcgCatalog.UnitTests.Application.Handlers.Review;

public class DeleteGameReviewHandlerTests
{
    [Fact]
    public async Task Handle_QuandoAvaliacaoNaoEncontrada_LancaNotFoundException()
    {
        // Arrange
        var reviewRepositoryMock = Substitute.For<IGameReviewRepository>();

        reviewRepositoryMock.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((GameReview?)null);

        var handler = new DeleteGameReviewHandler(reviewRepositoryMock);
        var command = new DeleteGameReviewCommand(Guid.NewGuid());

        // Act
        var action = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await Should.ThrowAsync<NotFoundException>(action);
        await reviewRepositoryMock.DidNotReceive().DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_QuandoAvaliacaoExistente_DeletaComSucesso()
    {
        // Arrange
        var reviewRepositoryMock = Substitute.For<IGameReviewRepository>();

        var review = new GameReview(Guid.NewGuid(), Guid.NewGuid(), 4, "Bom jogo!");

        reviewRepositoryMock.GetByIdAsync(review.Id, Arg.Any<CancellationToken>())
            .Returns(review);

        var handler = new DeleteGameReviewHandler(reviewRepositoryMock);
        var command = new DeleteGameReviewCommand(review.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeTrue();

        await reviewRepositoryMock.Received(1).DeleteAsync(review.Id, Arg.Any<CancellationToken>());
    }
}
