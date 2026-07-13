using FcgCatalog.Domain.Entities;
using FcgCatalog.Domain.Repositories.Orders;
using FcgCatalog.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace FcgCatalog.Infrastructure.Repositories.Orders;

public sealed class OrderRepository(FcgCatalogDbContext context) : IOrderRepository
{
    public void Add(Order order) => context.Orders.Add(order);

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => context.Orders.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
