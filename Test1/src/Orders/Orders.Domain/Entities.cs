namespace Orders.Domain.Entities;
public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomerId { get; set; }
    public List<OrderLine> Lines { get; set; } = new();
    public decimal Total { get; set; }
    public string Status { get; set; } = "CREATED";
}
public class OrderLine
{
    public int Id { get; set; }
    public Guid OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
