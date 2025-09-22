using System.Linq;

namespace Menu.Domain.Abstractions;
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(object id, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
    IQueryable<T> Query();
}
public interface IUnitOfWork
{
    IRepository<Menu.Domain.Entities.MenuItem> MenuItems { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
