using FcgCatalog.Application.Queries.Reviews;
using FcgCatalog.Application.Handlers.Reviews;
using FcgCatalog.Domain.Entities;
using FcgCatalog.Domain.Repositories.Games;
using NSubstitute;
using Shouldly;

namespace FcgCatalog.UnitTests.Application.Handlers.Review;

public class GetReviewsByGameIdHandlerTests
{
    [Fact]
    public async Task Handle_QuandoExistemAvaliacoes_RetornaListaMapeadaComSucesso()
    {
        // Arrange
        var reviewRepositoryMock = Substitute.For<IGameReviewRepository>();

        var jogoId = Guid.NewGuid();
        var reviews = new List<GameReview>
        {
            new(jogoId, Guid.NewGuid(), 5, "Incrível!"),
            new(jogoId, Guid.NewGuid(), 4, "Muito bom.")
        };

        reviewRepositoryMock.GetByGameIdAsync(jogoId, Arg.Any<CancellationToken>())
            .Returns(reviews.AsReadOnly());

        var handler = new GetReviewsByGameIdHandler(reviewRepositoryMock);
        var query = new GetReviewsByGameIdQuery(jogoId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(2);
        result.Value[0].Comentario.ShouldBe("Incrível!");
        result.Value[1].Comentario.ShouldBe("Muito bom.");

        await reviewRepositoryMock.Received(1).GetByGameIdAsync(jogoId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_QuandoNaoExistemAvaliacoes_RetornaListaVazia()
    {
        // Arrange
        var reviewRepositoryMock = Substitute.For<IGameReviewRepository>();
        var jogoId = Guid.NewGuid();

        reviewRepositoryMock.GetByGameIdAsync(jogoId, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<GameReview>().ToList().AsReadOnly());

        var handler = new GetReviewsByGameIdHandler(reviewRepositoryMock);
        var query = new GetReviewsByGameIdQuery(jogoId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.ShouldBeEmpty();

        await reviewRepositoryMock.Received(1).GetByGameIdAsync(jogoId, Arg.Any<CancellationToken>());
    }
}
