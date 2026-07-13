using MediatR;
using FcgCatalog.Application.Mappers.Games;
using FcgCatalog.Application.Queries.Games;
using FcgCatalog.Application.Responses.Games;
using FcgCatalog.Domain.Repositories.Games;
using FcgCatalog.SharedKernel.Exceptions;
using OperationResult;

namespace FcgCatalog.Application.Handlers.Games;

public sealed class GetGameByIdHandler(IGameRepository repository)
: IRequestHandler<GetGameByIdQuery, Result<GameResponse>>
{
    public async Task<Result<GameResponse>> Handle(GetGameByIdQuery request, CancellationToken cancellationToken)
    {
        var game = await repository.GetByIdAsNoTrackingAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Jogo {request.Id} não encontrado.");

        return Result.Success(game.ToResponse());
    }
}
