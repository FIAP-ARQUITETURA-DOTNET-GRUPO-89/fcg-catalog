using MediatR;
using FcgCatalog.Application.Commands.Reviews;
using FcgCatalog.Application.Mappers.Reviews;
using FcgCatalog.Application.Responses.Reviews;
using FcgCatalog.Domain.Entities;
using FcgCatalog.Domain.Repositories.Games;
using FcgCatalog.SharedKernel.Exceptions;
using Microsoft.Extensions.Logging;
using OperationResult;

namespace FcgCatalog.Application.Handlers.Reviews;

public sealed partial class CreateGameReviewCommandHandler(
    IGameReviewRepository reviewRepository,
    IGameRepository gameRepository,
    ILogger<CreateGameReviewCommandHandler> logger)
: IRequestHandler<CreateGameReviewCommand, Result<GameReviewResponse>>
{
    public async Task<Result<GameReviewResponse>> Handle(CreateGameReviewCommand request, CancellationToken cancellationToken)
    {
        LogStarted(logger, request.JogoId);

        var game = await gameRepository.GetByIdAsync(request.JogoId, cancellationToken);
        if (game is null)
        {
            LogGameNotFound(logger, request.JogoId);
            throw new NotFoundException($"Jogo {request.JogoId} não encontrado.");
        }

        var review = new GameReview(request.JogoId, request.UsuarioId, request.Nota, request.Comentario);

        await reviewRepository.AddAsync(review, cancellationToken);

        LogCreated(logger, review.Id, request.JogoId);

        return Result.Success(review.ToResponse());
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Iniciando a criação de avaliação para o jogo '{JogoId}'.")]
    private static partial void LogStarted(ILogger logger, Guid jogoId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Avaliação não realizada. Jogo {JogoId} não encontrado.")]
    private static partial void LogGameNotFound(ILogger logger, Guid jogoId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Avaliação '{ReviewId}' criada com sucesso para o jogo '{JogoId}'.")]
    private static partial void LogCreated(ILogger logger, Guid reviewId, Guid jogoId);
}
