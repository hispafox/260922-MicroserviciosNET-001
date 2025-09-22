using Microsoft.EntityFrameworkCore;
using Orders.Domain.Entities;

namespace Orders.Infrastructure.Persistence;

public class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> Lines => Set<OrderLine>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Order>(e => { e.HasKey(x => x.Id); e.Property(x => x.Total).HasColumnType("decimal(18,2)"); });
        b.Entity<OrderLine>(e => { e.HasKey(x => x.Id); e.HasOne<Order>().WithMany(x => x.Lines).HasForeignKey(x => x.OrderId); });
    }
}
