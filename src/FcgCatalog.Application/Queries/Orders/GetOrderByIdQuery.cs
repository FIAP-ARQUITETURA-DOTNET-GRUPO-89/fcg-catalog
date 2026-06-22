using MediatR;
using FcgCatalog.Application.Responses.Orders;
using FcgCatalog.SharedKernel.Validators;
using OperationResult;

namespace FcgCatalog.Application.Queries.Orders;

public record GetOrderByIdQuery(Guid Id) : IRequest<Result<GetOrderByIdResponse>>, IValidatableRequest
{
    public string? UserId { get; set; }
    public bool IsAdmin { get; set; }
}
