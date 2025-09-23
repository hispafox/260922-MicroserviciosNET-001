using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Menu.Domain.Entities;

namespace Menu.Infrastructure.Persistence;

public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> e)
    {
        e.HasKey(x => x.Id);
        e.Property(x => x.Name).HasMaxLength(200).IsRequired();
        e.Property(x => x.Price).HasColumnType("decimal(18,2)");
        e.HasIndex(x => x.Name);
    }
}
