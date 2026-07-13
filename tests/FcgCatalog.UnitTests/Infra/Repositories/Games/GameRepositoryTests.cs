using FcgCatalog.Domain.Entities;
using FcgCatalog.Domain.Enums;
using FcgCatalog.Infrastructure.Repositories.Games;
using FcgCatalog.UnitTests.TestHelpers.Factories;
using Shouldly;

namespace FcgCatalog.UnitTests.Infra.Repositories.Games;

public class GameRepositoryTests
{
    private static Game NewGame(string nome = "Halo", decimal preco = 199.90m)
        => new(nome, "desc", preco, new DateTime(2020, 1, 1), ClassificacaoEtaria.Dezesseis);

    [Fact]
    public async Task Add_DevePersistirJogo()
    {
        // Arrange
        using var ctx = InMemoryDbContextFactory.CreateContext();
        var repo = new GameRepository(ctx);

        var game = NewGame();

        // Act
        repo.Add(game);
        await repo.SaveChangesAsync();

        var stored = await repo.GetByIdAsync(game.Id);

        // Assert
        stored.ShouldNotBeNull();
        stored!.Nome.ShouldBe("Halo");
    }

    [Fact]
    public async Task ExistsByNameAsync_DeveIgnorarIdInformado()
    {
        // Arrange
        using var ctx = InMemoryDbContextFactory.CreateContext();
        var repo = new GameRepository(ctx);

        var game = NewGame("Halo");

        repo.Add(game);
        await repo.SaveChangesAsync();

        // Act
        var exists = await repo.ExistsByNameAsync("Halo");
        var existsIgnoringId = await repo.ExistsByNameAsync("Halo", game.Id);

        // Assert
        exists.ShouldBeTrue();
        existsIgnoringId.ShouldBeFalse();
    }

    [Fact]
    public async Task CountActiveAsync_DeveIgnorarInativos()
    {
        // Arrange
        using var ctx = InMemoryDbContextFactory.CreateContext();
        var repo = new GameRepository(ctx);

        var ativo = NewGame("A");
        var inativo = NewGame("B");
        inativo.Inativar();

        repo.Add(ativo);
        repo.Add(inativo);
        await repo.SaveChangesAsync();

        // Act
        var total = await repo.CountActiveAsync();

        // Assert
        total.ShouldBe(1);
    }

    [Fact]
    public async Task GetPagedAsNoTrackingAsync_DeveRetornarOrdenadoPorNome()
    {
        // Arrange
        using var ctx = InMemoryDbContextFactory.CreateContext();
        var repo = new GameRepository(ctx);

        repo.Add(NewGame("Banjo"));
        repo.Add(NewGame("Halo"));
        repo.Add(NewGame("Doom"));

        await repo.SaveChangesAsync();

        // Act
        var page = await repo.GetPagedAsNoTrackingAsync(1, 10);

        // Assert
        page.Select(g => g.Nome)
            .ShouldBe(new[] { "Banjo", "Doom", "Halo" });
    }
}
