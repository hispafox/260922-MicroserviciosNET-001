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
}
