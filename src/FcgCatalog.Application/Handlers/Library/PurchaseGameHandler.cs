using FgcGames.EventContracts.Events;
using MassTransit;
using MediatR;
using FcgCatalog.Application.Commands.Library;
using FcgCatalog.Application.Responses.Library;
using FcgCatalog.Domain.Entities;
using FcgCatalog.Domain.Repositories.Games;
using FcgCatalog.Domain.Repositories.Library;
using FcgCatalog.Domain.Repositories.Orders;
using FcgCatalog.SharedKernel.Exceptions;
using Microsoft.Extensions.Logging;
using OperationResult;

namespace FcgCatalog.Application.Handlers.Library;

public sealed partial class PurchaseGameHandler(
    IGameRepository gameRepository,
    IOrderRepository orderRepository,
    IUserGameRepository userGameRepository,
    IPublishEndpoint publishEndpoint,
    ILogger<PurchaseGameHandler> logger)
: IRequestHandler<PurchaseGameCommand, Result<PurchaseGameResponse>>
{
    public async Task<Result<PurchaseGameResponse>> Handle(PurchaseGameCommand request, CancellationToken cancellationToken)
    {
        var game = await gameRepository.GetByIdAsNoTrackingAsync(request.GameId, cancellationToken)
            ?? throw new NotFoundException($"Jogo {request.GameId} não encontrado ou indisponível.");

        if (await userGameRepository.ExistsAsync(request.UserId, request.GameId, cancellationToken))
        {
            throw new AlreadyExistsException("O usuário já possui este jogo na biblioteca.");
        }

        var order = new Order(request.UserId, request.GameId, game.Preco);
        orderRepository.Add(order);
        await orderRepository.SaveChangesAsync(cancellationToken);

        var evt = new OrderPlacedEvent(order.Id, order.UserId, order.GameId, order.Price, order.CreatedAt);
        await publishEndpoint.Publish(evt, cancellationToken);

        LogPurchaseInitiated(logger, order.Id, order.UserId, order.GameId, order.Price);

        return Result.Success(new PurchaseGameResponse(order.Id, order.GameId, order.Price));
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Compra iniciada. OrderId: {OrderId}, UserId: {UserId}, GameId: {GameId}, Price: {Price}.")]
    private static partial void LogPurchaseInitiated(ILogger logger, Guid orderId, Guid userId, Guid gameId, decimal price);
}
