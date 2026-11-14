using MediatR;
using FcgCatalog.Application.Interfaces;
using FcgCatalog.Application.Queries.Games;
using FcgCatalog.Application.Responses.Games;
using FcgCatalog.SharedKernel.Responses;
using OperationResult;

namespace FcgCatalog.Application.Handlers.Games;

public sealed class SearchGamesHandler(IGameSearchService search)
    : IRequestHandler<SearchGamesQuery, Result<PagedResponse<GameSearchResponse>>>
{
    public async Task<Result<PagedResponse<GameSearchResponse>>> Handle(
        SearchGamesQuery request,
        CancellationToken cancellationToken)
    {
        var response = await search.SearchAsync(
            request.Q.Trim(),
            request.Page,
            request.PageSize,
            cancellationToken);

        return Result.Success(response);
    }
}
