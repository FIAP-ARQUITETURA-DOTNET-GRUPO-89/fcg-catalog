using FcgCatalog.Application.Responses.Reviews;
using FcgCatalog.Domain.Entities;

namespace FcgCatalog.Application.Mappers.Reviews;

public static class GameReviewMapper
{
    public static GameReviewResponse ToResponse(this GameReview review) =>
        new(
            review.Id,
            review.JogoId,
            review.UsuarioId,
            review.Nota,
            review.Comentario,
            review.CreatedAt
        );
}
