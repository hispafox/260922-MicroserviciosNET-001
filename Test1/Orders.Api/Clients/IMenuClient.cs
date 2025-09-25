using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Orders.Api.Clients
{
    public interface IMenuClient
    {
        Task<MenuItemDto?> GetAsync(int id, CancellationToken ct = default);
        Task<List<MenuItemDto>> GetAllAsync(CancellationToken ct = default);
        Task<int> CreateAsync(CreateMenuItemDto dto, CancellationToken ct = default);
        Task UpdateAsync(int id, UpdateMenuItemDto dto, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
    }

    // DTOs para el cliente
    public class MenuItemDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public double Price { get; set; }
        public int Stock { get; set; }
    }

    public class CreateMenuItemDto
    {
        public string? Name { get; set; }
        public double Price { get; set; }
        public int Stock { get; set; }
    }

    public class UpdateMenuItemDto
    {
        public string? Name { get; set; }
        public double Price { get; set; }
        public int Stock { get; set; }
    }
}
