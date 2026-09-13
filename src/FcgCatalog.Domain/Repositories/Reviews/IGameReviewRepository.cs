using FcgCatalog.Domain.Entities;

namespace FcgCatalog.Domain.Repositories.Games;

public interface IGameReviewRepository
{
    /// <summary>
    /// Adiciona uma avaliação e comentário de um jogo ao repositório NoSQL.
    /// </summary>
    /// <param name="review">Avaliação a ser adicionada.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    Task AddAsync(GameReview review, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém uma avaliação pelo identificador.
    /// </summary>
    /// <param name="id">Identificador da avaliação.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>A avaliação encontrada ou null caso não exista.</returns>
    Task<GameReview?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém todas as avaliações e comentários associados a um jogo específico.
    /// </summary>
    /// <param name="jogoId">Identificador do jogo.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Coleção de avaliações encontradas para o jogo.</returns>
    Task<IReadOnlyList<GameReview>> GetByGameIdAsync(Guid jogoId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza uma avaliação existente no repositório NoSQL.
    /// </summary>
    /// <param name="review">Avaliação com os dados atualizados.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    Task UpdateAsync(GameReview review, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove uma avaliação do repositório NoSQL pelo identificador.
    /// </summary>
    /// <param name="id">Identificador da avaliação.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
