using MediatR;
using FcgCatalog.Application.Responses.Games;
using FcgCatalog.Domain.Enums;
using FcgCatalog.SharedKernel.Validators;
using OperationResult;

namespace FcgCatalog.Application.Commands.Games;

public record CreateGameCommand(
    string Nome,
    string Descricao,
    decimal Preco,
    DateTime DataLancamento,
    ClassificacaoEtaria ClassificacaoEtaria)
: IRequest<Result<GameResponse>>, IValidatableRequest;
