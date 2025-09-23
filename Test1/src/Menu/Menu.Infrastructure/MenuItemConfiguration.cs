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
        e.Property(x => x.Price)
            .HasColumnType("decimal(18,2)");
        e.ToTable(t => t.HasCheckConstraint("CK_MenuItems_Price_NonNegative", "[Price] >= 0"));
        e.Property(x => x.Stock)
            .HasDefaultValue(0);
        e.Property(x => x.UpdatedAt)
            .ValueGeneratedOnAddOrUpdate();
        e.HasIndex(x => x.Name)
            .IsUnique()
            .HasDatabaseName("IX_MenuItems_Name_Unique");
        e.HasIndex(x => x.Stock)
            .HasDatabaseName("IX_MenuItems_Stock");
        e.HasIndex(x => x.UpdatedAt)
            .HasDatabaseName("IX_MenuItems_UpdatedAt");
    }
}
