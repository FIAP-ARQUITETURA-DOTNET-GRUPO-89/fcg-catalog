using System.Text.Json.Serialization;
using MediatR;
using FcgCatalog.SharedKernel.Validators;
using OperationResult;

namespace FcgCatalog.Application.Commands.Reviews;

public record UpdateGameReviewCommand(
    int Nota,
    string Comentario
) : IRequest<Result<bool>>, IValidatableRequest
{
    [JsonIgnore]
    public Guid Id { get; set; }
}
