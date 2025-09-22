using Microsoft.EntityFrameworkCore;
using Analytics.Domain.Entities;

namespace Analytics.Infrastructure.Persistence;

public class AnalyticsDbContext : DbContext
{
    public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options) { }
    public DbSet<OrderMetrics> Metrics => Set<OrderMetrics>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<OrderMetrics>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Total).HasColumnType("decimal(18,2)");
            e.HasIndex(x => x.OrderId);
        });
    }
}
