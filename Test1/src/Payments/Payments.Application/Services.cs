namespace Payments.Application;
using Payments.Domain.Abstractions;
using Payments.Domain.Entities;

public class PaymentsService
{
    private readonly IUnitOfWork _uow;
    private readonly Random _rng = new();
    public PaymentsService(IUnitOfWork uow) => _uow = uow;

    public async Task<bool> AuthorizeAsync(Guid orderId, decimal total, CancellationToken ct)
    {
        var approved = _rng.NextDouble() < 0.8;
        var p = new Payment { OrderId = orderId, Status = approved ? "APPROVED" : "DECLINED" };
        await _uow.Payments.AddAsync(p, ct); await _uow.SaveChangesAsync(ct);
        return approved;
    }
}
