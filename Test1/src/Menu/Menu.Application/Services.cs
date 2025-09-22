using Menu.Domain.Abstractions;
using Menu.Domain.Entities;

namespace Menu.Application;
public record MenuItemDto(int Id, string Name, decimal Price, int Stock);
public record CreateMenuItem(string Name, decimal Price, int Stock);
public record UpdateMenuItem(string Name, decimal Price, int Stock);
public class MenuService
{
    private readonly IUnitOfWork _uow;
    public MenuService(IUnitOfWork uow) => _uow = uow;
    public async Task<int> CreateAsync(CreateMenuItem cmd, CancellationToken ct)
    {
        if (cmd.Price <= 0) throw new ArgumentException("Precio inválido");
        var e = new MenuItem { Name = cmd.Name, Price = cmd.Price, Stock = cmd.Stock };
        await _uow.MenuItems.AddAsync(e, ct);
        await _uow.SaveChangesAsync(ct);
        return e.Id;
    }
    public async Task UpdateAsync(int id, UpdateMenuItem cmd, CancellationToken ct)
    {
        var e = await _uow.MenuItems.GetByIdAsync(id, ct) ?? throw new KeyNotFoundException();
        if (cmd.Price <= 0) throw new ArgumentException("Precio inválido");
        e.Name = cmd.Name; e.Price = cmd.Price; e.Stock = cmd.Stock; e.UpdatedAt = DateTimeOffset.UtcNow;
        _uow.MenuItems.Update(e); await _uow.SaveChangesAsync(ct);
    }
}
