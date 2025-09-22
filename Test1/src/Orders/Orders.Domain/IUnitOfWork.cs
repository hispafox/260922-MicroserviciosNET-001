using System.Linq;

namespace Orders.Domain.Abstractions;
public interface IUnitOfWork
{
    IRepository<Orders.Domain.Entities.Order> Orders { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
