using FcgCatalog.Application.Handlers.Games;
using FcgCatalog.Application.Interfaces;
using FcgCatalog.Application.Queries.Games;
using FcgCatalog.Application.Responses.Games;
using FcgCatalog.Application.Validators.Games;
using FcgCatalog.Domain.Enums;
using FcgCatalog.SharedKernel.Responses;
using NSubstitute;
using Shouldly;

namespace FcgCatalog.UnitTests.Application.Handlers.Games;

public class SearchGamesHandlerTests
{
    [Fact]
    public async Task Handle_DeveDelegarBuscaAoMotorDeBusca()
    {
        // Arrange
        var search = Substitute.For<IGameSearchService>();

        var expected = new PagedResponse<GameSearchResponse>(
            [new GameSearchResponse(Guid.NewGuid(), "The Legend of Zelda", "Aventura", 299.90m, new DateTime(2017, 3, 3), ClassificacaoEtaria.Doze, 4.2)],
            1,
            1,
            10);

        search.SearchAsync("zeldda", 1, 10, Arg.Any<CancellationToken>())
            .Returns(expected);

        var handler = new SearchGamesHandler(search);

        // Act
        var result = await handler.Handle(
            new SearchGamesQuery("  zeldda  ", 1, 10),
            CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(expected);

        await search.Received(1)
            .SearchAsync("zeldda", 1, 10, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("", 1, 10)]
    [InlineData("zelda", 0, 10)]
    [InlineData("zelda", 1, 51)]
    [InlineData("zelda", 1001, 10)]
    public void Validator_QuandoParametrosInvalidos_DeveFalhar(string termo, int page, int pageSize)
    {
        var validator = new SearchGamesValidator();

        var result = validator.Validate(new SearchGamesQuery(termo, page, pageSize));

        result.IsValid.ShouldBeFalse();
    }
}
