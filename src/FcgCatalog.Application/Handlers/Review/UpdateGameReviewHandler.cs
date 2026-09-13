using MediatR;
using FcgCatalog.Application.Commands.Reviews;
using FcgCatalog.Domain.Repositories.Games;
using FcgCatalog.SharedKernel.Exceptions;
using OperationResult;

namespace FcgCatalog.Application.Handlers.Reviews;

public sealed class UpdateGameReviewHandler(
    IGameReviewRepository reviewRepository)
: IRequestHandler<UpdateGameReviewCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateGameReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await reviewRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Avaliação {request.Id} não encontrada.");

        review.Atualizar(request.Nota, request.Comentario);

        await reviewRepository.UpdateAsync(review, cancellationToken);

        return Result.Success(true);
    }
}
