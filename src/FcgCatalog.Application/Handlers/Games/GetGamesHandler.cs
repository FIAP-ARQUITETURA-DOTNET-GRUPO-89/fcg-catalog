using MediatR;
using FcgCatalog.Application.Interfaces;
using FcgCatalog.Application.Mappers.Games;
using FcgCatalog.Application.Queries.Games;
using FcgCatalog.Application.Responses.Games;
using FcgCatalog.Domain.Repositories.Games;
using FcgCatalog.SharedKernel.Responses;
using OperationResult;

namespace FcgCatalog.Application.Handlers.Games;

public sealed class GetGamesHandler(
    IGameRepository repository,
    IGameCacheService cache)
    : IRequestHandler<GetGamesQuery, Result<PagedResponse<GameResponse>>>
{
    public async Task<Result<PagedResponse<GameResponse>>> Handle(
        GetGamesQuery request,
        CancellationToken cancellationToken)
    {
        var cached = await cache.GetGamesAsync(
            request.Page,
            request.PageSize);

        if (cached is not null)
            return Result.Success(cached);

        var totalCount = await repository.CountActiveAsync(
            cancellationToken);

        var games = await repository.GetPagedAsNoTrackingAsync(
            request.Page,
            request.PageSize,
            cancellationToken);

        var items = games.Select(g => g.ToResponse());

        var response = new PagedResponse<GameResponse>(
            items,
            totalCount,
            request.Page,
            request.PageSize);

        await cache.SetGamesAsync(
            request.Page,
            request.PageSize,
            response);

        return Result.Success(response);
    }
}
