using MediatR;
using FcgCatalog.Application.Responses.Orders;
using FcgCatalog.SharedKernel.Responses;
using FcgCatalog.SharedKernel.Validators;
using OperationResult;

namespace FcgCatalog.Application.Queries.Orders;

public record GetOrdersQuery(int Page = 1, int PageSize = 10)
    : IRequest<Result<PagedResponse<GetOrdersResponse>>>, IValidatableRequest
{
    public string? UserId { get; set; }
    public bool IsAdmin { get; set; }
}
