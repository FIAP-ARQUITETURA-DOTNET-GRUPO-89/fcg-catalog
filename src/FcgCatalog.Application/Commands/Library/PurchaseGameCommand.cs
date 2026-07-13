using System.Text.Json.Serialization;
using MediatR;
using FcgCatalog.Application.Responses.Library;
using FcgCatalog.SharedKernel.Validators;
using OperationResult;

namespace FcgCatalog.Application.Commands.Library;

public record PurchaseGameCommand(Guid GameId)
: IRequest<Result<PurchaseGameResponse>>, IValidatableRequest
{
    [JsonIgnore]
    public Guid UserId { get; set; }
}
