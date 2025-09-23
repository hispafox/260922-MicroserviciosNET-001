using Bogus;
using Menu.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Menu.Infrastructure.Persistence;

public static class MenuSeeder
{
    public static async Task SeedAsync(MenuDbContext context)
    {
        if (await context.Items.AnyAsync()) return;

        var faker = new Faker<MenuItem>()
            .RuleFor(m => m.Name, (f, m) => f.UniqueIndex + "-" + f.Commerce.ProductName())
            .RuleFor(m => m.Description, f => f.Commerce.ProductDescription())
            .RuleFor(m => m.Price, f => f.Random.Decimal(1, 100))
            .RuleFor(m => m.Stock, f => f.Random.Int(0, 100))
            .RuleFor(m => m.CreatedAt, _ => DateTimeOffset.UtcNow)
            .RuleFor(m => m.UpdatedAt, _ => DateTimeOffset.UtcNow);

        var menus = faker.Generate(50);
        await context.Items.AddRangeAsync(menus);
        await context.SaveChangesAsync();
    }
}
