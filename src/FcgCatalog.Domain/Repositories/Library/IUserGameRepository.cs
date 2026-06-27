using FcgCatalog.Domain.Entities;

namespace FcgCatalog.Domain.Repositories.Library;

public interface IUserGameRepository
{
    void Add(UserGame userGame);
    Task<bool> ExistsAsync(Guid userId, Guid gameId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserGame>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
