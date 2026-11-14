using FcgCatalog.Application.Responses.Games;
using FcgCatalog.Domain.Entities;
using FcgCatalog.SharedKernel.Responses;

namespace FcgCatalog.Application.Interfaces;

/// <summary>
/// Define as operações do motor de busca de jogos (Elasticsearch/OpenSearch),
/// independente da consulta tradicional ao banco relacional.
/// </summary>
public interface IGameSearchService
{
    /// <summary>
    /// Obtém a quantidade de jogos indexados.
    /// </summary>
    /// <returns>A quantidade de documentos ou null quando o índice ainda não foi criado.</returns>
    Task<long?> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Insere ou atualiza o jogo no índice. Jogos inativos são removidos do índice.
    /// Escritas fora de ordem são descartadas pela versão do jogo.
    /// </summary>
    Task IndexAsync(Game game, CancellationToken cancellationToken = default);

    /// <summary>
    /// Constrói um novo índice a partir dos lotes informados e o publica atomicamente,
    /// sem deixar a busca sem índice durante a reconstrução.
    /// </summary>
    /// <returns>Quantidade de jogos indexados.</returns>
    Task<int> RebuildIndexAsync(
        IAsyncEnumerable<IReadOnlyCollection<Game>> batches,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca jogos ativos com tolerância a erros de digitação, ordenados por relevância.
    /// </summary>
    Task<PagedResponse<GameSearchResponse>> SearchAsync(
        string termo,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
