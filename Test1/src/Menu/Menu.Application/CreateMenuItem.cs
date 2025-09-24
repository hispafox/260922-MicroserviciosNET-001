using Menu.Domain.ValueObjects;

namespace Menu.Application;

public record CreateMenuItem(string Name, decimal Price, int Stock);
