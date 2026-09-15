using MediatR;
using FcgCatalog.Application.Responses.Reviews;
using FcgCatalog.SharedKernel.Validators;
using OperationResult;

namespace FcgCatalog.Application.Commands.Reviews;

public record CreateGameReviewCommand(
    Guid JogoId,
    Guid UsuarioId,
    int Nota,
    string Comentario
) : IRequest<Result<GameReviewResponse>>, IValidatableRequest;
