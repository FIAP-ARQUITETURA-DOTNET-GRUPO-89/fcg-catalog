using FcgCatalog.Domain.Entities;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using NSubstitute;

namespace FcgCatalog.UnitTests.Infra.Repositories.Games;

public class GameReviewRepositoryTests
{
    private readonly IConfiguration _configurationMock;
    private readonly IMongoDatabase _databaseMock;
    private readonly IMongoCollection<GameReview> _collectionMock;

    public GameReviewRepositoryTests()
    {
        _configurationMock = Substitute.For<IConfiguration>();
        _databaseMock = Substitute.For<IMongoDatabase>();
        _collectionMock = Substitute.For<IMongoCollection<GameReview>>();

        _configurationMock.GetConnectionString("mongodb").Returns("mongodb://localhost:27017/test_db");

    }

    [Fact]
    public async Task AddAsync_DeveInserirReviewNoBanco()
    {
        // Arrange
        var review = new GameReview(Guid.NewGuid(), Guid.NewGuid(), 5, "Excelente!");
    }
}
