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

public class ReindexGamesHandlerTests
{
    private readonly IGameRepository _repo = Substitute.For<IGameRepository>();
    private readonly IGameSearchService _search = Substitute.For<IGameSearchService>();

    private ReindexGamesHandler CreateHandler()
        => new(_repo, _search, NullLogger<ReindexGamesHandler>.Instance);

    private static Game NewGame(string nome)
        => new(nome, "desc", 100m, DateTime.Today, ClassificacaoEtaria.Livre);

    [Fact]
    public async Task Handle_DeveReconstruirIndiceLendoJogosPorCursor()
    {
        // Arrange
        IReadOnlyList<Game> batch1 = [NewGame("Halo"), NewGame("Zelda")];

        _repo.GetActiveBatchAfterAsNoTrackingAsync(null, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(batch1);
        _repo.GetActiveBatchAfterAsNoTrackingAsync("Zelda", Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns([]);
        _repo.GetChangedSinceAsNoTrackingAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns([]);

        var received = new List<IReadOnlyCollection<Game>>();

        _search.RebuildIndexAsync(Arg.Any<IAsyncEnumerable<IReadOnlyCollection<Game>>>(), Arg.Any<CancellationToken>())
            .Returns(async call =>
            {
                await foreach (var batch in call.Arg<IAsyncEnumerable<IReadOnlyCollection<Game>>>())
                {
                    received.Add(batch);
                }

                return received.Sum(b => b.Count);
            });

        // Act
        var result = await CreateHandler().Handle(new ReindexGamesCommand(), CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(2);
        received.ShouldHaveSingleItem().ShouldBe(batch1);
    }

    [Fact]
    public async Task Handle_DeveReaplicarJogosAlteradosDuranteAReconstrucao()
    {
        // Arrange
        var alterado = NewGame("Halo");
        alterado.Inativar();

        _search.RebuildIndexAsync(Arg.Any<IAsyncEnumerable<IReadOnlyCollection<Game>>>(), Arg.Any<CancellationToken>())
            .Returns(0);
        _repo.GetChangedSinceAsNoTrackingAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns([alterado]);

        // Act
        await CreateHandler().Handle(new ReindexGamesCommand(), CancellationToken.None);

        // Assert
        Received.InOrder(() =>
        {
            _search.RebuildIndexAsync(Arg.Any<IAsyncEnumerable<IReadOnlyCollection<Game>>>(), Arg.Any<CancellationToken>());
            _search.IndexAsync(alterado, Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task Handle_QuandoIndiceSincronizado_NaoDeveReconstruir()
    {
        // Arrange
        _search.CountAsync(Arg.Any<CancellationToken>()).Returns(3L);
        _repo.CountActiveAsync(Arg.Any<CancellationToken>()).Returns(3);

        // Act
        var result = await CreateHandler().Handle(
            new ReindexGamesCommand(OnlyIfOutOfSync: true),
            CancellationToken.None);

        // Assert
        result.Value.ShouldBe(3);
        await _search.DidNotReceiveWithAnyArgs().RebuildIndexAsync(default!, default);
    }

    [Theory]
    [InlineData(null, 3)]
    [InlineData(0L, 3)]
    public async Task Handle_QuandoIndiceAusenteOuDivergente_DeveReconstruir(long? indexed, int active)
    {
        // Arrange
        _search.CountAsync(Arg.Any<CancellationToken>()).Returns(indexed);
        _repo.CountActiveAsync(Arg.Any<CancellationToken>()).Returns(active);
        _repo.GetChangedSinceAsNoTrackingAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        await CreateHandler().Handle(
            new ReindexGamesCommand(OnlyIfOutOfSync: true),
            CancellationToken.None);

        // Assert
        await _search.Received(1).RebuildIndexAsync(
            Arg.Any<IAsyncEnumerable<IReadOnlyCollection<Game>>>(),
            Arg.Any<CancellationToken>());
    }
}
