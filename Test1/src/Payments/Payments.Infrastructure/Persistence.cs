using Microsoft.EntityFrameworkCore;
using Payments.Domain.Entities;
using Payments.Domain.Abstractions;

namespace Payments.Infrastructure.Persistence;
public class PaymentsDbContext : DbContext
{
    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options) { }
    public DbSet<Payment> Payments => Set<Payment>();
    protected override void OnModelCreating(ModelBuilder b) => b.Entity<Payment>(e => e.HasKey(x => x.Id));
}
public class EfRepository<T> : IRepository<T> where T : class
{
    protected readonly PaymentsDbContext _db;
    public EfRepository(PaymentsDbContext db) => _db = db;
    public Task<T?> GetByIdAsync(object id, CancellationToken ct = default) => _db.Set<T>().FindAsync(new[] { id }, ct).AsTask();
    public async Task AddAsync(T e, CancellationToken ct = default) => await _db.Set<T>().AddAsync(e, ct);
    public void Update(T e) => _db.Set<T>().Update(e);
    public IQueryable<T> Query() => _db.Set<T>().AsQueryable();
}
public class UnitOfWork : IUnitOfWork
{
    private readonly PaymentsDbContext _db;
    private IRepository<Payment>? _payments;
    public UnitOfWork(PaymentsDbContext db) => _db = db;
    public IRepository<Payment> Payments => _payments ??= new EfRepository<Payment>(_db);
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
