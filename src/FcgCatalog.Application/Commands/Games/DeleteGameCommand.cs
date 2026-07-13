using MediatR;
using OperationResult;

namespace FcgCatalog.Application.Commands.Games;

public record DeleteGameCommand(Guid Id) : IRequest<Result<bool>>;
