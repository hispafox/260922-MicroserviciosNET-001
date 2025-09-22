using Analytics.Infrastructure.Persistence;

namespace Analytics.Application;
public class AnalyticsService
{
    private readonly IOrderMetricsRepository _repo;
    public AnalyticsService(IOrderMetricsRepository repo) => _repo = repo;
    public async Task OnOrderCreated(Guid orderId, decimal total, CancellationToken ct)
    {
        var existing = await _repo.GetByOrderIdAsync(orderId, ct);
        if (existing is null)
        {
            await _repo.AddAsync(new Domain.Entities.OrderMetrics { OrderId = orderId, Total = total, PaymentStatus = "UNKNOWN" }, ct);
            await _repo.SaveAsync(ct);
        }
    }
    public async Task OnPaymentUpdated(Guid orderId, bool approved, CancellationToken ct)
    {
        var m = await _repo.GetByOrderIdAsync(orderId, ct);
        if (m is null) return;
        m.PaymentStatus = approved ? "APPROVED" : "DECLINED";
        await _repo.SaveAsync(ct);
    }
}
