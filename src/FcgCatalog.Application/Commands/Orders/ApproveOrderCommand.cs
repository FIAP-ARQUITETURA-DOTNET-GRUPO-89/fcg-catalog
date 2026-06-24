using MediatR;
using OperationResult;

namespace FcgCatalog.Application.Commands.Orders;

public sealed record ApproveOrderCommand(Guid OrderId): IRequest<Result>;
