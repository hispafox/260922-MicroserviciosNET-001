using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Menu.Domain.Entities;
using Menu.Infrastructure.Persistence;

namespace Menu.Infrastructure.Persistence;

public class MenuDbContext : DbContext
{
    public MenuDbContext(DbContextOptions<MenuDbContext> options) : base(options) { }
    public DbSet<MenuItem> Items => Set<MenuItem>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.ApplyConfiguration(new MenuItemConfiguration());
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Logging y errores detallados solo en desarrollo
            optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
        }
    }

    // Auditing automático de timestamps
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is IAuditableEntity auditableEntity)
            {
                if (entry.State == EntityState.Added)
                {
                    auditableEntity.CreatedAt = DateTimeOffset.UtcNow;
                }
                auditableEntity.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }
    }
}
