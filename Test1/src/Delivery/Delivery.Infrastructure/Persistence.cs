using Microsoft.EntityFrameworkCore;
using Delivery.Domain.Entities;

namespace Delivery.Infrastructure.Persistence;
public class DeliveryDbContext : DbContext
{
    public DeliveryDbContext(DbContextOptions<DeliveryDbContext> options) : base(options) { }
    public DbSet<DeliveryOrder> Deliveries => Set<DeliveryOrder>();
    protected override void OnModelCreating(ModelBuilder b) => b.Entity<DeliveryOrder>(e => e.HasKey(x => x.Id));
}
public interface IDeliveryRepository
{
    Task<DeliveryOrder?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task AddAsync(DeliveryOrder d, CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
}
public class DeliveryRepository : IDeliveryRepository
{
    private readonly DeliveryDbContext _db;
    public DeliveryRepository(DeliveryDbContext db) => _db = db;
    public Task<DeliveryOrder?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default) => _db.Deliveries.FirstOrDefaultAsync(x => x.OrderId == orderId, ct);
    public async Task AddAsync(DeliveryOrder d, CancellationToken ct = default) => await _db.Deliveries.AddAsync(d, ct);
    public Task SaveAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
