using MediatR;
using FcgCatalog.Application.Interfaces;
using FcgCatalog.Application.Mappers.Games;
using FcgCatalog.Application.Queries.Games;
using FcgCatalog.Application.Responses.Games;
using FcgCatalog.Domain.Repositories.Games;
using FcgCatalog.SharedKernel.Exceptions;
using OperationResult;

namespace FcgCatalog.Application.Handlers.Games;

public sealed class GetGameByIdHandler(
    IGameRepository repository,
    IGameCacheService cache)
    : IRequestHandler<GetGameByIdQuery, Result<GameResponse>>
{
    public async Task<Result<GameResponse>> Handle(
        GetGameByIdQuery request,
        CancellationToken cancellationToken)
    {
        var cached = await cache.GetGameByIdAsync(
            request.Id);

        if (cached is not null)
            return Result.Success(cached);

        var game = await repository.GetByIdAsNoTrackingAsync(
            request.Id,
            cancellationToken)
            ?? throw new NotFoundException(
                $"Jogo {request.Id} não encontrado.");

        var response = game.ToResponse();

        await cache.SetGameByIdAsync(
            request.Id,
            response);

        return Result.Success(response);
    }
}
