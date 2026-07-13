using FcgCatalog.Domain.Entities;

namespace FcgCatalog.Domain.Repositories.Library;

public interface IUserGameRepository
{
    /// <summary>
    /// Adiciona um jogo à biblioteca do usuário.
    /// </summary>
    /// <param name="userGame">Relacionamento entre usuário e jogo.</param>
    void Add(UserGame userGame);

    /// <summary>
    /// Verifica se um jogo já pertence à biblioteca do usuário.
    /// </summary>
    /// <param name="userId">Identificador do usuário.</param>
    /// <param name="gameId">Identificador do jogo.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>True quando o jogo já estiver na biblioteca; caso contrário, false.</returns>
    Task<bool> ExistsAsync(Guid userId, Guid gameId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todos os jogos da biblioteca de um usuário.
    /// </summary>
    /// <param name="userId">Identificador do usuário.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Lista de jogos pertencentes à biblioteca do usuário.</returns>
    Task<IReadOnlyList<UserGame>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persiste as alterações realizadas no repositório.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Quantidade de registros afetados.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
