using FcgCatalog.Application.Responses.Games;
using FcgCatalog.SharedKernel.Responses;

namespace FcgCatalog.Application.Interfaces;

/// <summary>
/// Define as operações de leitura, armazenamento e invalidação do cache de jogos.
/// </summary>
public interface IGameCacheService
{
    /// <summary>
    /// Obtém a lista paginada de jogos armazenada no cache.
    /// </summary>
    Task<PagedResponse<GameResponse>?> GetGamesAsync(int page, int pageSize);

    /// <summary>
    /// Armazena a lista paginada de jogos no cache.
    /// </summary>
    Task SetGamesAsync(int page, int pageSize, PagedResponse<GameResponse> response);

    /// <summary>
    /// Obtém um jogo pelo identificador armazenado no cache.
    /// </summary>
    Task<GameResponse?> GetGameByIdAsync(Guid id);

    /// <summary>
    /// Armazena um jogo no cache pelo identificador.
    /// </summary>
    Task SetGameByIdAsync(Guid id, GameResponse response);

    /// <summary>
    /// Invalida o cache de um jogo e da lista de jogos.
    /// </summary>
    Task InvalidateGameAsync(Guid id);

    /// <summary>
    /// Invalida o cache da lista de jogos incrementando sua versão.
    /// </summary>
    Task InvalidateGameListAsync();
}
