using MediatR;
using FcgCatalog.Application.Responses.Games;
using FcgCatalog.SharedKernel.Responses;
using FcgCatalog.SharedKernel.Validators;
using OperationResult;

namespace FcgCatalog.Application.Queries.Games;

public record GetGamesQuery(int Page = 1, int PageSize = 10)
: IRequest<Result<PagedResponse<GameResponse>>>, IValidatableRequest;
