using Delivery.Infrastructure.Persistence;
using Delivery.Domain.Entities;

namespace Delivery.Application;
public class DeliveryService
{
    private readonly IDeliveryRepository _repo;
    public DeliveryService(IDeliveryRepository repo) => _repo = repo;
    public async Task AssignAsync(Guid orderId, CancellationToken ct)
    {
        var ex = await _repo.GetByOrderIdAsync(orderId, ct);
        if (ex is null) await _repo.AddAsync(new DeliveryOrder { OrderId = orderId, Status = "ASSIGNED" }, ct);
        else { ex.Status = "ASSIGNED"; }
        await _repo.SaveAsync(ct);
    }
}
