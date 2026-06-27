using System.Text.Json.Serialization;
using MediatR;
using FcgCatalog.SharedKernel.Validators;
using OperationResult;

namespace FcgCatalog.Application.Commands.Games;

public record UpdatePriceCommand(decimal NovoPreco)
: IRequest<Result<bool>>, IValidatableRequest
{
    [JsonIgnore]
    public Guid Id { get; set; }
}
