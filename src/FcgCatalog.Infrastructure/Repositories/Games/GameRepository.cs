using FcgCatalog.Domain.Entities;
using FcgCatalog.Domain.Repositories.Games;
using FcgCatalog.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace FcgCatalog.Infrastructure.Repositories.Games;

public sealed class GameRepository(FcgCatalogDbContext context) : IGameRepository
{
    public void Add(Game game) => context.Games.Add(game);

    public Task<Game?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => context.Games.FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

    public Task<Game?> GetByIdAsNoTrackingAsync(Guid id, CancellationToken cancellationToken = default)
        => context.Games.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id && !g.Inativo, cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, Game>> GetByIdsAsNoTrackingAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.Distinct().ToArray();
        if (idList.Length == 0)
        {
            return new Dictionary<Guid, Game>();
        }

        var games = await context.Games
            .AsNoTracking()
            .Where(g => idList.Contains(g.Id))
            .ToListAsync(cancellationToken);

        return games.ToDictionary(g => g.Id);
    }

    public Task<bool> ExistsByNameAsync(string nome, Guid? ignoreId = null, CancellationToken cancellationToken = default)
        => context.Games
            .AsNoTracking()
            .AnyAsync(g => g.Nome.ToLower() == nome.ToLower() && (ignoreId == null || g.Id != ignoreId), cancellationToken);

    public Task<int> CountActiveAsync(CancellationToken cancellationToken = default)
        => context.Games.CountAsync(g => !g.Inativo, cancellationToken);

    public async Task<IReadOnlyList<Game>> GetPagedAsNoTrackingAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        => await context.Games
            .AsNoTracking()
            .Where(g => !g.Inativo)
            .OrderBy(g => g.Nome)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
