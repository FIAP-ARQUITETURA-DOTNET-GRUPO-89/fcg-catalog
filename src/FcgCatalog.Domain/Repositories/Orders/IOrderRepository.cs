using FcgCatalog.Domain.Entities;

namespace FcgCatalog.Domain.Repositories.Orders;

public interface IOrderRepository
{
    void Add(Order order);
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
