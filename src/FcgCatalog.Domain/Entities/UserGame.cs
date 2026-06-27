using FcgCatalog.SharedKernel.Exceptions;

namespace FcgCatalog.Domain.Entities;

/// <summary>
/// Representa um jogo presente na biblioteca de um usuário.
/// </summary>
public class UserGame : BaseEntity
{
    protected UserGame() { }

    public UserGame(Guid userId, Guid gameId, decimal price)
    {
        if (userId == Guid.Empty)
        {
            throw new BusinessException("UserId é obrigatório.");
        }

        if (gameId == Guid.Empty)
        {
            throw new BusinessException("GameId é obrigatório.");
        }

        UserId = userId;
        GameId = gameId;
        Price = price;
        AcquiredAt = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }
    public Guid GameId { get; private set; }
    public decimal Price { get; private set; }
    public DateTime AcquiredAt { get; private set; }
}
