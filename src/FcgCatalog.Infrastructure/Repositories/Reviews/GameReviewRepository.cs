using FcgCatalog.Domain.Entities;
using FcgCatalog.Domain.Repositories.Games;
using MongoDB.Driver;

namespace FcgCatalog.Infrastructure.Repositories.Games;

public sealed class GameReviewRepository : IGameReviewRepository
{
    private readonly IMongoCollection<GameReview> _reviewsCollection;

    public GameReviewRepository(IMongoDatabase database)
    {
        _reviewsCollection = database.GetCollection<GameReview>("game_reviews");
    }

    public async Task AddAsync(GameReview review, CancellationToken cancellationToken = default)
    {
        await _reviewsCollection.InsertOneAsync(review, null, cancellationToken);
    }

    public async Task<GameReview?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<GameReview>.Filter.Eq(r => r.Id, id);
        return await _reviewsCollection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<GameReview>> GetByGameIdAsync(Guid jogoId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<GameReview>.Filter.Eq(r => r.JogoId, jogoId);
        var reviews = await _reviewsCollection.Find(filter).ToListAsync(cancellationToken);
        return reviews.AsReadOnly();
    }

    public async Task UpdateAsync(GameReview review, CancellationToken cancellationToken = default)
    {
        var filter = Builders<GameReview>.Filter.Eq(r => r.Id, review.Id);
        await _reviewsCollection.ReplaceOneAsync(filter, review, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var filter = Builders<GameReview>.Filter.Eq(r => r.Id, id);
        await _reviewsCollection.DeleteOneAsync(filter, cancellationToken);
    }
}
