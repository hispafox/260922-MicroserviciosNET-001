using Microsoft.EntityFrameworkCore;
using Analytics.Domain.Entities;

namespace Analytics.Infrastructure.Persistence;
public class OrderMetricsRepository : IOrderMetricsRepository
{
    private readonly AnalyticsDbContext _db;
    public OrderMetricsRepository(AnalyticsDbContext db) => _db = db;
    public Task<OrderMetrics?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        => _db.Metrics.FirstOrDefaultAsync(x => x.OrderId == orderId, ct);
    public async Task AddAsync(OrderMetrics m, CancellationToken ct = default) => await _db.Metrics.AddAsync(m, ct);
    public Task SaveAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
