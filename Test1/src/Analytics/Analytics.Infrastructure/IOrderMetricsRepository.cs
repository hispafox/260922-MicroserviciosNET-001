using Analytics.Domain.Entities;

namespace Analytics.Infrastructure.Persistence;

public interface IOrderMetricsRepository
{
    Task<OrderMetrics?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
    Task AddAsync(OrderMetrics m, CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
}
