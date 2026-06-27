using MediatR;
using FcgCatalog.Application.Commands.Library;
using FcgCatalog.Domain.Entities;
using FcgCatalog.Domain.Enums;
using FcgCatalog.Domain.Repositories.Library;
using FcgCatalog.Domain.Repositories.Orders;
using Microsoft.Extensions.Logging;
using OperationResult;
using ContractStatus = FgcGames.EventContracts.Enums.PaymentStatus;

namespace FcgCatalog.Application.Handlers.Library;

public sealed partial class ProcessPaymentResultHandler(
    IOrderRepository orderRepository,
    IUserGameRepository userGameRepository,
    ILogger<ProcessPaymentResultHandler> logger)
: IRequestHandler<ProcessPaymentResultCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ProcessPaymentResultCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            LogOrderNotFound(logger, request.OrderId);
            return Result.Success(true);
        }

        if (order.Status != OrderStatus.PendingPayment)
        {
            LogAlreadyProcessed(logger, order.Id, order.Status);
            return Result.Success(true);
        }

        if (request.Status == ContractStatus.Approved)
        {
            if (!await userGameRepository.ExistsAsync(order.UserId, order.GameId, cancellationToken))
            {
                userGameRepository.Add(new UserGame(order.UserId, order.GameId, order.Price));
            }

            order.Approve();
        }
        else
        {
            order.Reject();
        }

        await orderRepository.SaveChangesAsync(cancellationToken);

        LogProcessed(logger, order.Id, order.Status);
        return Result.Success(true);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Resultado de pagamento processado. OrderId: {OrderId}, Status: {Status}.")]
    private static partial void LogProcessed(ILogger logger, Guid orderId, OrderStatus status);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Order não encontrado ao processar PaymentProcessedEvent. OrderId: {OrderId}.")]
    private static partial void LogOrderNotFound(ILogger logger, Guid orderId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Order já processado anteriormente. OrderId: {OrderId}, Status atual: {Status}.")]
    private static partial void LogAlreadyProcessed(ILogger logger, Guid orderId, OrderStatus status);
}
