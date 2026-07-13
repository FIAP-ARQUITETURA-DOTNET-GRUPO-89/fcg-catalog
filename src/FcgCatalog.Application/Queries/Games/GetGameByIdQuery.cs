using MediatR;
using FcgCatalog.Application.Responses.Games;
using OperationResult;

namespace FcgCatalog.Application.Queries.Games;

public record GetGameByIdQuery(Guid Id) : IRequest<Result<GameResponse>>;
