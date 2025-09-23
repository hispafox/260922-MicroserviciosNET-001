using System.ComponentModel.DataAnnotations;
using Menu.Domain.Exceptions;

namespace Menu.Domain.Entities;
public class MenuItem(int id, string name, decimal price, int stock, DateTimeOffset updatedAt) : IValidatableObject
{
    public MenuItem() : this(0, "", 0m, 0, DateTimeOffset.UtcNow) {}

    public int Id { get; set; } = id;
    public string Name { get; set; } = name;
    public decimal Price { get; set; } = price;
    public int Stock { get; set; } = stock;
    public DateTimeOffset UpdatedAt { get; set; } = updatedAt;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Price <= 0)
            yield return new ValidationResult("El precio debe ser mayor a cero", new[] { nameof(Price) });

        if (string.IsNullOrWhiteSpace(Name))
            yield return new ValidationResult("El nombre es obligatorio", new[] { nameof(Name) });

        if (Name.Length > 100)
            yield return new ValidationResult("El nombre no debe exceder 100 caracteres", new[] { nameof(Name) });

        if (Stock < 0)
            yield return new ValidationResult("El stock no puede ser negativo", new[] { nameof(Stock) });

        if (UpdatedAt > DateTimeOffset.UtcNow)
            yield return new ValidationResult("La fecha de actualización no puede ser futura", new[] { nameof(UpdatedAt) });
    }

    // Método de negocio para actualizar el precio
    public void ActualizarPrecio(decimal nuevoPrecio)
    {
        if (nuevoPrecio <= 0)
            throw new InvalidPriceException(nuevoPrecio);
        Price = nuevoPrecio;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    // Método de negocio para descontar stock
    public void DescontarStock(int cantidad)
    {
        if (cantidad <= 0)
            throw new ArgumentException("La cantidad debe ser mayor a cero.", nameof(cantidad));
        if (Stock - cantidad < 0)
            throw new InvalidOperationException("No hay suficiente stock para descontar la cantidad solicitada.");
        Stock -= cantidad;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

