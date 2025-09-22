using Orders.Domain.Entities;
using Orders.Domain.Abstractions;

namespace Orders.Infrastructure.Persistence;
public class UnitOfWork : IUnitOfWork
{
    private readonly OrdersDbContext _db;
    private IRepository<Order>? _orders;
    public UnitOfWork(OrdersDbContext db) => _db = db;
    public IRepository<Order> Orders => _orders ??= new EfRepository<Order>(_db);
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
