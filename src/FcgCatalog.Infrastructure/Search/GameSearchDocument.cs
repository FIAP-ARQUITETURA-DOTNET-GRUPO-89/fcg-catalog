using FcgCatalog.Domain.Entities;

namespace FcgCatalog.Infrastructure.Search;

/// <summary>
/// Projeção de leitura do jogo armazenada no índice de busca.
/// </summary>
public sealed class GameSearchDocument
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = null!;
    public string Descricao { get; set; } = null!;
    public decimal Preco { get; set; }
    public DateTime DataLancamento { get; set; }
    public int ClassificacaoEtaria { get; set; }

    public static GameSearchDocument FromGame(Game game) => new()
    {
        Id = game.Id,
        Nome = game.Nome,
        Descricao = game.Descricao,
        Preco = game.Preco,
        DataLancamento = game.DataLancamento,
        ClassificacaoEtaria = (int)game.ClassificacaoEtaria
    };
}
