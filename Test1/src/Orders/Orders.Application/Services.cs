using Orders.Domain.Abstractions;
using Orders.Domain.Entities;

namespace Orders.Application;
public record OrderLineCreate(int ProductId, int Quantity, decimal Price);
public record CreateOrder(Guid CustomerId, List<OrderLineCreate> Lines);
public class OrdersService
{
    private readonly IUnitOfWork _uow;
    public OrdersService(IUnitOfWork uow) { _uow = uow; }
    public async Task<Guid> CreateAsync(CreateOrder cmd, CancellationToken ct)
    {
        var o = new Order { CustomerId = cmd.CustomerId };
        foreach (var l in cmd.Lines)
            o.Lines.Add(new OrderLine { ProductId = l.ProductId, Quantity = l.Quantity, Price = l.Price });
        o.Total = o.Lines.Sum(x => x.Price * x.Quantity);
        await _uow.Orders.AddAsync(o, ct); await _uow.SaveChangesAsync(ct);
        return o.Id;
    }
    public async Task<object?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var o = await _uow.Orders.GetByIdAsync(id, ct); if (o is null) return null;
        return new { o.Id, o.CustomerId, o.Total, o.Status, Lines = o.Lines.Select(l => new { l.ProductId, l.Quantity, l.Price }) };
    }
}
