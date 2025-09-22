using System.Linq;

namespace Menu.Domain.Abstractions;
public interface IUnitOfWork
{
    IRepository<Menu.Domain.Entities.MenuItem> MenuItemRepository { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
