using MediatR;
using FcgCatalog.Application.Commands.Orders;
using FcgCatalog.Domain.Repositories.Orders;
using FcgCatalog.SharedKernel.Exceptions;
using Microsoft.Extensions.Logging;
using OperationResult;

public sealed partial class ApproveOrderHandler(
    IOrderRepository orderRepository,
    ILogger<ApproveOrderHandler> logger)
: IRequestHandler<ApproveOrderCommand, Result>
{
    public async Task<Result> Handle(ApproveOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order is null)
        {
            LogOrderNotFound(logger, request.OrderId);
            return new NotFoundException($"Pedido {request.OrderId} não encontrado.");
        }

        order.Approve();

        await orderRepository.SaveChangesAsync();

        LogOrderApproved(logger, request.OrderId);

        return Result.Success();
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Pedido {OrderId} aprovado com sucesso")]
    private static partial void LogOrderApproved(ILogger logger, Guid orderId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Pedido {OrderId} não encontrado")]
    private static partial void LogOrderNotFound(ILogger logger, Guid orderId);
}
