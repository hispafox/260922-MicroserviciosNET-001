namespace Delivery.Domain.Entities;
public class DeliveryOrder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public string Status { get; set; } = "PENDING";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
