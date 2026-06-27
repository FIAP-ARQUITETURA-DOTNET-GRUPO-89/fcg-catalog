using FcgCatalog.Domain.Entities;
using FcgCatalog.Domain.Enums;
using FcgCatalog.Infrastructure.Database;

namespace FcgCatalog.IntegrationTests.TestHelpers;

/// <summary>
/// Responsável por popular o banco de dados com dados iniciais necessários para os testes de integração.
/// </summary>
public static class TestDataSeeder
{
    public static async Task SeedAsync(FcgCatalogDbContext context)
    {
        if (context.Games.Any())
        {
            return;
        }

        var game = new Game(
            nome: "Jogo Teste",
            descricao: "Descrição do jogo de teste para integração.",
            preco: 99.90m,
            dataLancamento: new DateTime(2024, 1, 1),
            classificacaoEtaria: ClassificacaoEtaria.Livre
        );

        context.Games.Add(game);

        await context.SaveChangesAsync();
    }
}
