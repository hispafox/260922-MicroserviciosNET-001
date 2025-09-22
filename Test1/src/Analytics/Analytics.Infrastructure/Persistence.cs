using Microsoft.EntityFrameworkCore;
using Analytics.Domain.Entities;

namespace Analytics.Infrastructure.Persistence;
public class AnalyticsDbContext : DbContext
{
    public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options) { }
    public DbSet<OrderMetrics> Metrics => Set<OrderMetrics>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<OrderMetrics>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Total).HasColumnType("decimal(18,2)");
            e.HasIndex(x => x.OrderId);
        });
    }
}
public interface IOrderMetricsRepository
{
    Task<OrderMetrics?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task AddAsync(OrderMetrics m, CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
}
public class OrderMetricsRepository : IOrderMetricsRepository
{
    private readonly AnalyticsDbContext _db;
    public OrderMetricsRepository(AnalyticsDbContext db) => _db = db;
    public Task<OrderMetrics?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        => _db.Metrics.FirstOrDefaultAsync(x => x.OrderId == orderId, ct);
    public async Task AddAsync(OrderMetrics m, CancellationToken ct = default) => await _db.Metrics.AddAsync(m, ct);
    public Task SaveAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
