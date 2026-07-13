using MediatR;
using FcgCatalog.Application.Responses.Library;
using OperationResult;

namespace FcgCatalog.Application.Queries.Library;

public record GetUserLibraryQuery(Guid UserId) : IRequest<Result<IReadOnlyList<UserGameResponse>>>;
