using FcgCatalog.Domain.Entities;
using FcgCatalog.Domain.Repositories.Library;
using FcgCatalog.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace FcgCatalog.Infrastructure.Repositories.Library;

public sealed class UserGameRepository(FcgCatalogDbContext context) : IUserGameRepository
{
    public void Add(UserGame userGame) => context.UserGames.Add(userGame);

    public Task<bool> ExistsAsync(Guid userId, Guid gameId, CancellationToken cancellationToken = default)
        => context.UserGames.AsNoTracking()
            .AnyAsync(x => x.UserId == userId && x.GameId == gameId, cancellationToken);

    public async Task<IReadOnlyList<UserGame>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await context.UserGames.AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.AcquiredAt)
            .ToListAsync(cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
