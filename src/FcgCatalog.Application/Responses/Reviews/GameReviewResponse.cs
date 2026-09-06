namespace FcgCatalog.Application.Responses.Reviews;

public record GameReviewResponse(
    Guid Id,
    Guid JogoId,
    Guid UsuarioId,
    int Nota,
    string Comentario,
    DateTime CreatedAt
);
