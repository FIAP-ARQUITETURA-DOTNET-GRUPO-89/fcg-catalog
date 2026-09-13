using MediatR;
using FcgCatalog.Application.Responses.Reviews;
using OperationResult;

namespace FcgCatalog.Application.Queries.Reviews;

public record GetReviewsByGameIdQuery(Guid JogoId) : IRequest<Result<IReadOnlyList<GameReviewResponse>>>;
