using System;
using System.Globalization;
using Menu.Domain.Exceptions;

namespace Menu.Domain.ValueObjects;

public sealed class Price : IEquatable<Price>
{
    public decimal Value { get; }

    public Price(decimal value)
    {
        if (value <= 0)
            throw new InvalidPriceException(value);
        Value = value;
    }

    public static implicit operator decimal(Price price) => price.Value;
    public static explicit operator Price(decimal value) => new Price(value);

    public override string ToString() => Value.ToString("C", CultureInfo.CurrentCulture);

    public bool Equals(Price? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is Price other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
}
