using Orders.Domain.Abstractions;

namespace Orders.Infrastructure.Persistence;

public class EfRepository<T> : IRepository<T> where T : class
{
    protected readonly OrdersDbContext _db;
    public EfRepository(OrdersDbContext db) => _db = db;
    public Task<T?> GetByIdAsync(object id, CancellationToken ct = default) => _db.Set<T>().FindAsync(new[] { id }, ct).AsTask();
    public async Task AddAsync(T e, CancellationToken ct = default) => await _db.Set<T>().AddAsync(e, ct);
    public void Update(T e) => _db.Set<T>().Update(e);
    public IQueryable<T> Query() => _db.Set<T>().AsQueryable();
}
