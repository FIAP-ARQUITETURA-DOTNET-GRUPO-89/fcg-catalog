using FcgCatalog.Domain.Entities;

namespace FcgCatalog.Domain.Repositories.Games;

public interface IGameRepository
{
    void Add(Game game);
    Task<Game?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Game?> GetByIdAsNoTrackingAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<Guid, Game>> GetByIdsAsNoTrackingAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string nome, Guid? ignoreId = null, CancellationToken cancellationToken = default);
    Task<int> CountActiveAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Game>> GetPagedAsNoTrackingAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
