using System.Text.Json;
using FcgCatalog.Application.Interfaces;
using FcgCatalog.Application.Responses.Games;
using FcgCatalog.SharedKernel.Responses;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace FcgCatalog.Infrastructure.Caching;

public sealed class RedisGameCacheService(
    IConnectionMultiplexer redis,
    ILogger<RedisGameCacheService> logger)
    : IGameCacheService
{
    private const string VersionKey = "fcg:catalog:games:version";
    private const string GameKeyPrefix = "fcg:catalog:game";

    private static readonly TimeSpan CacheDuration =
        TimeSpan.FromMinutes(5);

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly IDatabase _database = redis.GetDatabase();

    public async Task<PagedResponse<GameResponse>?> GetGamesAsync(
        int page,
        int pageSize)
    {
        try
        {
            var version = await GetVersionAsync();

            var key = BuildGamesKey(
                version,
                page,
                pageSize);

            var cached = await _database.StringGetAsync(key);

            if (!cached.HasValue)
                return null;

            return JsonSerializer.Deserialize<PagedResponse<GameResponse>>(
                cached.ToString(),
                JsonOptions);
        }
        catch (RedisException ex)
        {
            logger.LogWarning(
                ex,
                "Não foi possível ler a lista de jogos do Redis.");

            return null;
        }
    }

    public async Task SetGamesAsync(
        int page,
        int pageSize,
        PagedResponse<GameResponse> response)
    {
        try
        {
            var version = await GetVersionAsync();

            var key = BuildGamesKey(
                version,
                page,
                pageSize);

            var json = JsonSerializer.Serialize(
                response,
                JsonOptions);

            await _database.StringSetAsync(
                key,
                json,
                CacheDuration);
        }
        catch (RedisException ex)
        {
            logger.LogWarning(
                ex,
                "Não foi possível armazenar a lista de jogos no Redis.");
        }
    }

    public async Task<GameResponse?> GetGameByIdAsync(Guid id)
    {
        try
        {
            var cached = await _database.StringGetAsync(
                BuildGameKey(id));

            if (!cached.HasValue)
                return null;

            return JsonSerializer.Deserialize<GameResponse>(
                cached.ToString(),
                JsonOptions);
        }
        catch (RedisException ex)
        {
            logger.LogWarning(
                ex,
                "Não foi possível ler o jogo {GameId} do Redis.",
                id);

            return null;
        }
    }

    public async Task SetGameByIdAsync(
        Guid id,
        GameResponse response)
    {
        try
        {
            var json = JsonSerializer.Serialize(
                response,
                JsonOptions);

            await _database.StringSetAsync(
                BuildGameKey(id),
                json,
                CacheDuration);
        }
        catch (RedisException ex)
        {
            logger.LogWarning(
                ex,
                "Não foi possível armazenar o jogo {GameId} no Redis.",
                id);
        }
    }

    public async Task InvalidateGameAsync(Guid id)
    {
        try
        {
            await _database.KeyDeleteAsync(
                BuildGameKey(id));
        }
        catch (RedisException ex)
        {
            logger.LogWarning(
                ex,
                "Não foi possível remover o jogo {GameId} do Redis.",
                id);
        }

        await InvalidateGameListAsync();
    }

    public async Task InvalidateGameListAsync()
    {
        try
        {
            await _database.StringIncrementAsync(
                VersionKey);
        }
        catch (RedisException ex)
        {
            logger.LogWarning(
                ex,
                "Não foi possível invalidar o cache da lista de jogos.");
        }
    }

    private async Task<long> GetVersionAsync()
    {
        var value = await _database.StringGetAsync(
            VersionKey);

        if (value.HasValue &&
            long.TryParse(value.ToString(), out var version))
        {
            return version;
        }

        await _database.StringSetAsync(
            VersionKey,
            1,
            when: When.NotExists);

        value = await _database.StringGetAsync(
            VersionKey);

        return value.HasValue &&
               long.TryParse(value.ToString(), out version)
            ? version
            : 1;
    }

    private static string BuildGamesKey(
        long version,
        int page,
        int pageSize)
        => $"fcg:catalog:games:v{version}:page:{page}:size:{pageSize}";

    private static string BuildGameKey(Guid id)
        => $"{GameKeyPrefix}:{id}";
}
