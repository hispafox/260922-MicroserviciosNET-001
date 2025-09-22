namespace Analytics.Domain.Entities;
public class OrderMetrics
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public decimal Total { get; set; }
    public string PaymentStatus { get; set; } = "UNKNOWN";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
