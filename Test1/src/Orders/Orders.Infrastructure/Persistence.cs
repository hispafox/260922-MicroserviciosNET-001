using Microsoft.EntityFrameworkCore;
using Orders.Domain.Entities;
using Orders.Domain.Abstractions;

namespace Orders.Infrastructure.Persistence;
public class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> Lines => Set<OrderLine>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Order>(e => { e.HasKey(x => x.Id); e.Property(x => x.Total).HasColumnType("decimal(18,2)"); });
        b.Entity<OrderLine>(e => { e.HasKey(x => x.Id); e.HasOne<Order>().WithMany(x => x.Lines).HasForeignKey(x => x.OrderId); });
    }
}
public class EfRepository<T> : IRepository<T> where T : class
{
    protected readonly OrdersDbContext _db;
    public EfRepository(OrdersDbContext db) => _db = db;
    public Task<T?> GetByIdAsync(object id, CancellationToken ct = default) => _db.Set<T>().FindAsync(new[] { id }, ct).AsTask();
    public async Task AddAsync(T e, CancellationToken ct = default) => await _db.Set<T>().AddAsync(e, ct);
    public void Update(T e) => _db.Set<T>().Update(e);
    public IQueryable<T> Query() => _db.Set<T>().AsQueryable();
}
public class UnitOfWork : IUnitOfWork
{
    private readonly OrdersDbContext _db;
    private IRepository<Order>? _orders;
    public UnitOfWork(OrdersDbContext db) => _db = db;
    public IRepository<Order> Orders => _orders ??= new EfRepository<Order>(_db);
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
