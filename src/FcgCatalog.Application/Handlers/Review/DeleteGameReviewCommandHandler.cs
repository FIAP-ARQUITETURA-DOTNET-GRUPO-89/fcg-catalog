using MediatR;
using FcgCatalog.Application.Commands.Reviews;
using FcgCatalog.Domain.Repositories.Games;
using FcgCatalog.SharedKernel.Exceptions;
using OperationResult;

namespace FcgCatalog.Application.Handlers.Reviews;

public sealed class DeleteGameReviewCommandHandler(
    IGameReviewRepository reviewRepository)
: IRequestHandler<DeleteGameReviewCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteGameReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await reviewRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Avaliação {request.Id} não encontrada.");

        await reviewRepository.DeleteAsync(request.Id, cancellationToken);

        return Result.Success(true);
    }
}
