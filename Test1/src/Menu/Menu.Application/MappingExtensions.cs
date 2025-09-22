using Menu.Domain.Entities;

namespace Menu.Application;
public static class MappingExtensions
{
    public static MenuItem ToEntity(this CreateMenuItem dto)
    {
        return new MenuItem
        {
            Name = dto.Name,
            Price = dto.Price,
            Stock = dto.Stock,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public static void ApplyUpdate(this UpdateMenuItem dto, MenuItem entity)
    {
        entity.Name = dto.Name;
        entity.Price = dto.Price;
        entity.Stock = dto.Stock;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
    }
}
