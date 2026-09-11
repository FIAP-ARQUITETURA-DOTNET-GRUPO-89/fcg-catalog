using FcgCatalog.Application.Responses.Games;
using FcgCatalog.SharedKernel.Responses;

namespace FcgCatalog.Application.Interfaces;

public interface IGameCacheService
{
    Task<PagedResponse<GameResponse>?> GetGamesAsync(
        int page,
        int pageSize);

    Task SetGamesAsync(
        int page,
        int pageSize,
        PagedResponse<GameResponse> response);

    Task<GameResponse?> GetGameByIdAsync(Guid id);

    Task SetGameByIdAsync(
        Guid id,
        GameResponse response);

    Task InvalidateGameAsync(Guid id);

    Task InvalidateGameListAsync();
}
