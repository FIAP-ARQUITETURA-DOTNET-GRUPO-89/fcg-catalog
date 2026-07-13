using FcgCatalog.Domain.Entities;

namespace FcgCatalog.Domain.Repositories.Orders;

public interface IOrderRepository
{
    /// <summary>
    /// Adiciona um pedido ao repositório.
    /// </summary>
    /// <param name="order">Pedido a ser adicionado.</param>
    void Add(Order order);

    /// <summary>
    /// Obtém um pedido pelo identificador.
    /// </summary>
    /// <param name="id">Identificador do pedido.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>O pedido encontrado ou null caso não exista.</returns>
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persiste as alterações realizadas no repositório.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Quantidade de registros afetados.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
