using System.Runtime.CompilerServices;
using MediatR;
using FcgCatalog.Application.Commands.Games;
using FcgCatalog.Application.Interfaces;
using FcgCatalog.Domain.Entities;
using FcgCatalog.Domain.Repositories.Games;
using Microsoft.Extensions.Logging;
using OperationResult;

namespace FcgCatalog.Application.Handlers.Games;

public sealed partial class ReindexGamesHandler(
    IGameRepository repository,
    IGameSearchService search,
    ILogger<ReindexGamesHandler> logger)
    : IRequestHandler<ReindexGamesCommand, Result<int>>
{
    private const int BatchSize = 500;

    // Margem para diferenças de relógio entre as instâncias que gravam UpdatedAt.
    private static readonly TimeSpan ClockSkew = TimeSpan.FromMinutes(1);

    public async Task<Result<int>> Handle(
        ReindexGamesCommand request,
        CancellationToken cancellationToken)
    {
        if (request.OnlyIfOutOfSync)
        {
            var indexed = await search.CountAsync(cancellationToken);
            var active = await repository.CountActiveAsync(cancellationToken);

            if (indexed == active)
            {
                return Result.Success(active);
            }
        }

        var startedAt = DateTime.UtcNow - ClockSkew;

        var total = await search.RebuildIndexAsync(
            ReadActiveGamesAsync(cancellationToken),
            cancellationToken);

        // Escritas feitas durante a reconstrução foram para o índice anterior;
        // reaplica no índice publicado os jogos alterados desde o início.
        var changed = await repository.GetChangedSinceAsNoTrackingAsync(
            startedAt,
            cancellationToken);

        foreach (var game in changed)
        {
            await search.IndexAsync(game, cancellationToken);
        }

        LogReindexed(logger, total, changed.Count);

        return Result.Success(total);
    }

    // Paginação por cursor (nome único): inativações durante a leitura não deslocam as páginas.
    private async IAsyncEnumerable<IReadOnlyCollection<Game>> ReadActiveGamesAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        string? afterNome = null;

        while (true)
        {
            var games = await repository.GetActiveBatchAfterAsNoTrackingAsync(
                afterNome,
                BatchSize,
                cancellationToken);

            if (games.Count == 0)
            {
                yield break;
            }

            yield return games;

            afterNome = games[^1].Nome;
        }
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Reindexação concluída. {Total} jogos indexados e {Changed} alterações reaplicadas.")]
    private static partial void LogReindexed(
        ILogger logger,
        int total,
        int changed);
}
