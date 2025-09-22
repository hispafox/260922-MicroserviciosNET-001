using Menu.Domain.Abstractions;
using Menu.Domain.Entities;
using Menu.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Menu.Infrastructure.Persistence;

public class EfRepository<T> : IRepository<T> where T : class
{
    protected readonly MenuDbContext _db;
    public EfRepository(MenuDbContext db) => _db = db;
    public Task<T?> GetByIdAsync(object id, CancellationToken ct = default) => _db.Set<T>().FindAsync(new[] { id }, ct).AsTask();
    public async Task AddAsync(T e, CancellationToken ct = default) => await _db.Set<T>().AddAsync(e, ct);
    public void Update(T e) => _db.Set<T>().Update(e);
    public void Remove(T e) => _db.Set<T>().Remove(e);
    public IQueryable<T> Query() => _db.Set<T>().AsQueryable();
}

public class MenuItemRepository : EfRepository<MenuItem>, IMenuItemRepository
{
    public MenuItemRepository(MenuDbContext db) : base(db) {}

    public async Task<MenuItem> GetByIdAsync(object id, CancellationToken ct = default)
    {
        var item = await base.GetByIdAsync(id, ct);
        if (item == null)
            throw new EntityNotFoundException(nameof(MenuItem), id);
        return item;
    }
    public async Task AddAsync(MenuItem e, CancellationToken ct = default) => await base.AddAsync(e, ct);
    public void Update(MenuItem e) => base.Update(e);
    public void Remove(MenuItem e) => base.Remove(e);
    public IQueryable<MenuItem> Query() => base.Query();

    public async Task<IEnumerable<MenuItem>> GetAvailableAsync()
    {
        return await _db.Items.Where(x => x.Stock > 0).ToListAsync();
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _db.Items.AnyAsync(x => x.Name == name);
    }
}
