using FcgCatalog.Domain.Entities;

namespace FcgCatalog.Domain.Repositories.Games;

public interface IGameRepository
{
    /// <summary>
    /// Adiciona um jogo ao repositório.
    /// </summary>
    /// <param name="game">Jogo a ser adicionado.</param>
    void Add(Game game);

    /// <summary>
    /// Obtém um jogo pelo identificador.
    /// </summary>
    /// <param name="id">Identificador do jogo.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>O jogo encontrado ou null caso não exista.</returns>
    Task<Game?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém um jogo pelo identificador sem rastreamento pelo Entity Framework.
    /// </summary>
    /// <param name="id">Identificador do jogo.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>O jogo encontrado ou null caso não exista.</returns>
    Task<Game?> GetByIdAsNoTrackingAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém uma coleção de jogos pelos identificadores informados sem rastreamento pelo Entity Framework.
    /// </summary>
    /// <param name="ids">Identificadores dos jogos.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Dicionário contendo os jogos encontrados indexados pelo identificador.</returns>
    Task<IReadOnlyDictionary<Guid, Game>> GetByIdsAsNoTrackingAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se já existe um jogo cadastrado com o nome informado.
    /// </summary>
    /// <param name="nome">Nome do jogo.</param>
    /// <param name="ignoreId">Identificador do jogo que deve ser desconsiderado na validação.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>True quando existir um jogo com o mesmo nome; caso contrário, false.</returns>
    Task<bool> ExistsByNameAsync(string nome, Guid? ignoreId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém a quantidade de jogos ativos cadastrados.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Quantidade de jogos ativos.</returns>
    Task<int> CountActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém uma lista paginada de jogos ativos sem rastreamento pelo Entity Framework.
    /// </summary>
    /// <param name="page">Número da página.</param>
    /// <param name="pageSize">Quantidade de registros por página.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Lista de jogos.</returns>
    Task<IReadOnlyList<Game>> GetPagedAsNoTrackingAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persiste as alterações realizadas no repositório.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Quantidade de registros afetados.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
