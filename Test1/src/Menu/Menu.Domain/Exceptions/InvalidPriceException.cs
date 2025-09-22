using System;

namespace Menu.Domain.Exceptions;

public class InvalidPriceException : DomainException
{
    public InvalidPriceException(decimal value)
        : base($"El precio '{value}' no es válido.", "INVALID_PRICE")
    {
    }
}