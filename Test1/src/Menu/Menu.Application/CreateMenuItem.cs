using Menu.Domain.ValueObjects;

namespace Menu.Application;

public record CreateMenuItem(string Name, Price Price, int Stock);
