using System.Linq;

namespace Payments.Domain.Abstractions;
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(object id, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    IQueryable<T> Query();
}
public interface IUnitOfWork
{
    IRepository<Payments.Domain.Entities.Payment> Payments { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
