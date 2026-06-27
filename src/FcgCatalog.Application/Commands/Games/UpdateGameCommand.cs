using System.Text.Json.Serialization;
using MediatR;
using FcgCatalog.Domain.Enums;
using FcgCatalog.SharedKernel.Validators;
using OperationResult;

namespace FcgCatalog.Application.Commands.Games;

public record UpdateGameCommand(
    string Nome,
    string Descricao,
    decimal Preco,
    DateTime DataLancamento,
    ClassificacaoEtaria ClassificacaoEtaria)
: IRequest<Result<bool>>, IValidatableRequest
{
    [JsonIgnore]
    public Guid Id { get; set; }
}
