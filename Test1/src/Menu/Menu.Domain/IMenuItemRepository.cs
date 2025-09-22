using Menu.Domain.Entities;

namespace Menu.Domain.Abstractions;

public interface IMenuItemRepository : IRepository<MenuItem>
{
    Task<IEnumerable<MenuItem>> GetAvailableAsync();
    Task<bool> ExistsByNameAsync(string name);
}
