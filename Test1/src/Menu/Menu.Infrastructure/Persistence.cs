using Microsoft.EntityFrameworkCore;
using Menu.Domain.Entities;
using Menu.Domain.Abstractions;

namespace Menu.Infrastructure.Persistence;

public class MenuDbContext : DbContext
{
    public MenuDbContext(DbContextOptions<MenuDbContext> options) : base(options) { }
    public DbSet<MenuItem> Items => Set<MenuItem>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<MenuItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Price).HasColumnType("decimal(18,2)");
            e.HasIndex(x => x.Name);
        });
    }
}

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

public class UnitOfWork : IUnitOfWork
{
    private readonly MenuDbContext _db;
    private IRepository<MenuItem>? _menu;
    public UnitOfWork(MenuDbContext db) => _db = db;
    public IRepository<MenuItem> MenuItems => _menu ??= new EfRepository<MenuItem>(_db);
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
