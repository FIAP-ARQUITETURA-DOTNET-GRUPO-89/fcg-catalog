using MediatR;
using FcgCatalog.Application.Queries.Library;
using FcgCatalog.Application.Responses.Library;
using FcgCatalog.Domain.Repositories.Games;
using FcgCatalog.Domain.Repositories.Library;
using OperationResult;

namespace FcgCatalog.Application.Handlers.Library;

public sealed class GetUserLibraryHandler(
    IUserGameRepository userGameRepository,
    IGameRepository gameRepository)
: IRequestHandler<GetUserLibraryQuery, Result<IReadOnlyList<UserGameResponse>>>
{
    public async Task<Result<IReadOnlyList<UserGameResponse>>> Handle(GetUserLibraryQuery request, CancellationToken cancellationToken)
    {
        var items = await userGameRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        if (items.Count == 0)
        {
            return Result.Success<IReadOnlyList<UserGameResponse>>([]);
        }

        var games = await gameRepository.GetByIdsAsNoTrackingAsync(items.Select(i => i.GameId), cancellationToken);

        var responses = items
            .Select(item => new UserGameResponse(
                item.GameId,
                games.TryGetValue(item.GameId, out var game) ? game.Nome : string.Empty,
                item.Price,
                item.AcquiredAt))
            .ToList();

        return Result.Success<IReadOnlyList<UserGameResponse>>(responses);
    }
}
