using FcgCatalog.Domain.Enums;
using FcgCatalog.SharedKernel.Exceptions;

namespace FcgCatalog.Domain.Entities;

/// <summary>
/// Representa o ciclo de uma compra de um jogo por um usuário. Cada compra
/// publica um <c>OrderPlacedEvent</c> e é finalizada (Approved/Rejected)
/// ao consumir um <c>PaymentProcessedEvent</c>.
/// </summary>
public class Order : BaseEntity
{
    protected Order() { }

    public Order(Guid userId, Guid gameId, decimal price)
    {
        if (userId == Guid.Empty)
        {
            throw new InvalidOrderException("UserId é obrigatório.");
        }

        if (gameId == Guid.Empty)
        {
            throw new InvalidOrderException("GameId é obrigatório.");
        }

        if (price <= 0)
        {
            throw new InvalidOrderException("Preço inválido para a compra.");
        }

        UserId = userId;
        GameId = gameId;
        Price = price;
        Status = OrderStatus.PendingPayment;
    }

    public Guid UserId { get; private set; }
    public Guid GameId { get; private set; }
    public decimal Price { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    public void Approve()
    {
        if (Status == OrderStatus.Approved)
        {
            return;
        }

        EnsurePending();

        Status = OrderStatus.Approved;
        ProcessedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void Reject()
    {
        if (Status == OrderStatus.Rejected)
        {
            return;
        }

        EnsurePending();

        Status = OrderStatus.Rejected;
        ProcessedAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    private void EnsurePending()
    {
        if (Status != OrderStatus.PendingPayment)
        {
            throw new InvalidOrderException($"A compra já foi processada com o status '{Status}'.");
        }
    }
}
