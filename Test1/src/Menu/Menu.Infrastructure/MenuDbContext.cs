using Microsoft.EntityFrameworkCore;
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
}
