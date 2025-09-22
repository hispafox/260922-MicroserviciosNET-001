using Menu.Domain.Entities;
using Menu.Domain.Abstractions;

namespace Menu.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly MenuDbContext _db;
    private IRepository<MenuItem>? _menu;
    public UnitOfWork(MenuDbContext db) => _db = db;
    public IRepository<MenuItem> MenuItems => _menu ??= new EfRepository<MenuItem>(_db);
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
