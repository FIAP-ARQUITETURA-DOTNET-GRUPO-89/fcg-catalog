using FcgCatalog.Application.Interfaces;
using FcgCatalog.Application.Responses.Games;
using FcgCatalog.Domain.Entities;
using FcgCatalog.Domain.Enums;
using FcgCatalog.SharedKernel.Exceptions;
using FcgCatalog.SharedKernel.Responses;
using FcgCatalog.SharedKernel.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenSearch.Client;
using OpenSearch.Net;

namespace FcgCatalog.Infrastructure.Search;

/// <summary>
/// Busca de jogos no OpenSearch. Leituras e escritas usam o alias <see cref="OpenSearchSettings.IndexName"/>,
/// que aponta para um índice físico versionado ("fcg-games-{timestamp}"). A reindexação constrói um novo
/// índice e troca o alias atomicamente, então o alias nunca fica sem índice ou com mapeamento dinâmico.
/// </summary>
public sealed class OpenSearchGameSearchService(
    IOpenSearchClient client,
    IOptions<OpenSearchSettings> options,
    ILogger<OpenSearchGameSearchService> logger)
    : IGameSearchService
{
    private const string Analyzer = "pt_folded";

    private readonly OpenSearchSettings _settings = options.Value;

    private string Alias => _settings.IndexName;

    public async Task<long?> CountAsync(CancellationToken cancellationToken = default)
    {
        if ((await GetAliasedIndicesAsync(cancellationToken)).Count == 0)
        {
            return null;
        }

        var response = await client.CountAsync<GameSearchDocument>(
            c => c.Index(Alias),
            cancellationToken);

        if (!response.IsValid)
        {
            throw new InvalidOperationException(
                $"Não foi possível contar os documentos do índice '{Alias}': {response.DebugInformation}");
        }

        return response.Count;
    }

    public async Task IndexAsync(Game game, CancellationToken cancellationToken = default)
    {
        // Falhas no motor de busca não devem impedir o CRUD; a reindexação corrige divergências.
        try
        {
            var version = GetVersion(game);

            if (game.Inativo)
            {
                var deleteResponse = await client.DeleteAsync(
                    new DeleteRequest(Alias, game.Id.ToString())
                    {
                        Version = version,
                        VersionType = VersionType.ExternalGte
                    },
                    cancellationToken);

                if (!deleteResponse.IsValid &&
                    deleteResponse.Result != Result.NotFound &&
                    !IsVersionConflict(deleteResponse))
                {
                    LogIndexFailure(logger, game.Id, deleteResponse.DebugInformation);
                }

                return;
            }

            // RequireAlias: sem o alias, a escrita falha em vez de criar um índice com mapeamento dinâmico.
            var response = await client.IndexAsync(
                GameSearchDocument.FromGame(game),
                i => i
                    .Index(Alias)
                    .Id(game.Id.ToString())
                    .Version(version)
                    .VersionType(VersionType.ExternalGte)
                    .RequireAlias(),
                cancellationToken);

            if (!response.IsValid && !IsVersionConflict(response))
            {
                LogIndexFailure(logger, game.Id, response.DebugInformation);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(
                ex,
                "Não foi possível sincronizar o jogo {GameId} com o índice de busca.",
                game.Id);
        }
    }

    public async Task<int> RebuildIndexAsync(
        IAsyncEnumerable<IReadOnlyCollection<Game>> batches,
        CancellationToken cancellationToken = default)
    {
        var newIndex = $"{Alias}-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid().ToString("N")[..8]}";

        await CreateIndexAsync(newIndex, cancellationToken);

        var published = false;

        try
        {
            var total = 0;

            await foreach (var batch in batches.WithCancellation(cancellationToken))
            {
                total += await BulkIndexAsync(newIndex, batch, cancellationToken);
            }

            var previous = await GetAliasedIndicesAsync(cancellationToken);

            await SwapAliasAsync(newIndex, cancellationToken);
            published = true;

            // Remove apenas os índices que estavam publicados; índices em construção por
            // outra instância não estão no alias e não são afetados.
            foreach (var index in previous.Where(i => i != newIndex))
            {
                var delete = await client.Indices.DeleteAsync(index, ct: cancellationToken);

                if (!delete.IsValid && delete.ApiCall?.HttpStatusCode != 404)
                {
                    logger.LogWarning(
                        "Não foi possível remover o índice antigo {Index}: {Details}",
                        index,
                        delete.DebugInformation);
                }
            }

            return total;
        }
        catch when (!published)
        {
            await client.Indices.DeleteAsync(newIndex, ct: CancellationToken.None);
            throw;
        }
    }

    public async Task<PagedResponse<GameSearchResponse>> SearchAsync(
        string termo,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var response = await client.SearchAsync<GameSearchDocument>(s => s
            .Index(Alias)
            .From((page - 1) * pageSize)
            .Size(pageSize)
            .TrackTotalHits()
            .Query(q => q
                .Bool(b => b
                    .Should(
                        sh => sh.MultiMatch(mm => mm
                            .Fields(f => f
                                .Field(d => d.Nome, 3)
                                .Field(d => d.Descricao))
                            .Query(termo)
                            .Fuzziness(Fuzziness.Auto)
                            .PrefixLength(1)),
                        // Favorece jogos cujo nome começa com o termo digitado.
                        sh => sh.MatchPhrasePrefix(mp => mp
                            .Field(d => d.Nome)
                            .Query(termo)
                            .Boost(2)))
                    .MinimumShouldMatch(1)))
            .Sort(so => so
                .Descending(SortSpecialField.Score)
                .Ascending("nome.keyword")),
            cancellationToken);

        if (!response.IsValid)
        {
            if (IsUnavailable(response))
            {
                logger.LogWarning("Busca indisponível: {Details}", response.DebugInformation);
                throw new ServiceUnavailableException(
                    "A busca de jogos está temporariamente indisponível. Tente novamente em instantes.");
            }

            throw new InvalidOperationException(
                $"Falha ao consultar o índice de busca: {response.DebugInformation}");
        }

        var items = response.Hits
            .Select(h => new GameSearchResponse(
                h.Source.Id,
                h.Source.Nome,
                h.Source.Descricao,
                h.Source.Preco,
                h.Source.DataLancamento,
                (ClassificacaoEtaria)h.Source.ClassificacaoEtaria,
                h.Score))
            .ToList();

        return new PagedResponse<GameSearchResponse>(
            items,
            (int)response.Total,
            page,
            pageSize);
    }

    private async Task<int> BulkIndexAsync(
        string index,
        IReadOnlyCollection<Game> games,
        CancellationToken cancellationToken)
    {
        var versions = games
            .Where(g => !g.Inativo)
            .ToDictionary(g => g.Id, GetVersion);

        if (versions.Count == 0)
        {
            return 0;
        }

        var response = await client.BulkAsync(
            b => b
                .Index(index)
                .IndexMany(
                    games.Where(g => !g.Inativo).Select(GameSearchDocument.FromGame),
                    (d, doc) => d
                        .Id(doc.Id.ToString())
                        .Version(versions[doc.Id])
                        .VersionType(VersionType.ExternalGte)),
            cancellationToken);

        if (!response.IsValid || response.Errors)
        {
            throw new InvalidOperationException(
                $"Falha ao indexar jogos em lote: {response.DebugInformation}");
        }

        return versions.Count;
    }

    private async Task<IReadOnlyCollection<string>> GetAliasedIndicesAsync(CancellationToken cancellationToken)
    {
        var response = await client.Indices.GetAliasAsync(
            Indices.All,
            a => a.Name(Alias),
            cancellationToken);

        if (response.ApiCall?.HttpStatusCode == 404)
        {
            return [];
        }

        if (!response.IsValid)
        {
            throw new InvalidOperationException(
                $"Não foi possível consultar o alias '{Alias}': {response.DebugInformation}");
        }

        return response.Indices.Keys.Select(k => k.Name).ToList();
    }

    private async Task SwapAliasAsync(string newIndex, CancellationToken cancellationToken)
    {
        var actions = new List<IAliasAction>
        {
            // Remove o alias de qualquer índice da família, inclusive os publicados por outra instância.
            new AliasRemoveAction
            {
                Remove = new AliasRemoveOperation { Index = $"{Alias}-*", Alias = Alias, MustExist = false }
            }
        };

        // Índice concreto com o nome do alias (criado por versões anteriores) impediria a criação do alias.
        if (await IsConcreteIndexAsync(Alias, cancellationToken))
        {
            actions.Add(new AliasRemoveIndexAction { RemoveIndex = new AliasRemoveIndexOperation { Index = Alias } });
        }

        actions.Add(new AliasAddAction { Add = new AliasAddOperation { Index = newIndex, Alias = Alias } });

        var response = await client.Indices.BulkAliasAsync(
            new BulkAliasRequest { Actions = actions },
            cancellationToken);

        if (!response.IsValid)
        {
            throw new InvalidOperationException(
                $"Não foi possível publicar o índice '{newIndex}' no alias '{Alias}': {response.DebugInformation}");
        }
    }

    private async Task<bool> IsConcreteIndexAsync(string name, CancellationToken cancellationToken)
    {
        var response = await client.Indices.GetAsync(name, ct: cancellationToken);

        return response.IsValid && response.Indices.Keys.Any(k => k.Name == name);
    }

    private async Task CreateIndexAsync(string index, CancellationToken cancellationToken)
    {
        var response = await client.Indices.CreateAsync(index, c => c
            .Settings(s => s
                .NumberOfShards(1)
                .NumberOfReplicas(_settings.NumberOfReplicas)
                .Analysis(a => a
                    .TokenFilters(tf => tf
                        .Stemmer("pt_stemmer", st => st.Language("light_portuguese")))
                    .Analyzers(an => an
                        .Custom(Analyzer, ca => ca
                            .Tokenizer("standard")
                            .Filters("lowercase", "asciifolding", "pt_stemmer")))))
            .Map<GameSearchDocument>(m => m
                .Properties(p => p
                    .Keyword(k => k.Name(d => d.Id))
                    .Text(t => t
                        .Name(d => d.Nome)
                        .Analyzer(Analyzer)
                        .Fields(f => f.Keyword(k => k.Name("keyword").IgnoreAbove(256))))
                    .Text(t => t
                        .Name(d => d.Descricao)
                        .Analyzer(Analyzer))
                    .Number(n => n
                        .Name(d => d.Preco)
                        .Type(NumberType.ScaledFloat)
                        .ScalingFactor(100))
                    .Date(d => d.Name(x => x.DataLancamento))
                    .Number(n => n
                        .Name(d => d.ClassificacaoEtaria)
                        .Type(NumberType.Integer)))),
            cancellationToken);

        if (!response.IsValid)
        {
            throw new InvalidOperationException(
                $"Não foi possível criar o índice '{index}': {response.DebugInformation}");
        }
    }

    // Versão externa = última alteração em microssegundos (precisão do PostgreSQL),
    // para que uma escrita com dados antigos não sobrescreva uma mais nova.
    private static long GetVersion(Game game)
        => (game.UpdatedAt ?? game.CreatedAt).Ticks / 10;

    // Sem conexão, índice ainda não publicado ou erro do cluster.
    private static bool IsUnavailable(IResponse response)
        => response.ApiCall?.HttpStatusCode is null or 404 or >= 500;

    private static bool IsVersionConflict(IResponse response)
        => response.ApiCall?.HttpStatusCode == 409;

    private static void LogIndexFailure(ILogger logger, Guid gameId, string details)
        => logger.LogWarning(
            "Falha ao sincronizar o jogo {GameId} com o índice de busca: {Details}",
            gameId,
            details);
}
