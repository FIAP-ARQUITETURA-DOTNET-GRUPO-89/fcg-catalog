using MediatR;
using FcgCatalog.Application.Mappers.Reviews;
using FcgCatalog.Application.Queries.Reviews;
using FcgCatalog.Application.Responses.Reviews;
using FcgCatalog.Domain.Repositories.Games;
using OperationResult;

namespace FcgCatalog.Application.Handlers.Reviews;

public sealed class GetReviewsByGameIdHandler(
    IGameReviewRepository reviewRepository)
: IRequestHandler<GetReviewsByGameIdQuery, Result<IReadOnlyList<GameReviewResponse>>>
{
    public async Task<Result<IReadOnlyList<GameReviewResponse>>> Handle(GetReviewsByGameIdQuery request, CancellationToken cancellationToken)
    {
        var reviews = await reviewRepository.GetByGameIdAsync(request.JogoId, cancellationToken);

        var response = reviews.Select(r => r.ToResponse()).ToList().AsReadOnly();

        return Result.Success<IReadOnlyList<GameReviewResponse>>(response);
    }
}
