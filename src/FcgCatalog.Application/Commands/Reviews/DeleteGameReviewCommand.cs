using MediatR;
using OperationResult;

namespace FcgCatalog.Application.Commands.Reviews;

public record DeleteGameReviewCommand(Guid Id) : IRequest<Result<bool>>;
