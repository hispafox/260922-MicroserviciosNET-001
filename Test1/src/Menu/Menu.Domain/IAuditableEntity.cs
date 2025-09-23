using System;

namespace Menu.Domain.Entities
{
    public interface IAuditableEntity
    {
        DateTimeOffset CreatedAt { get; set; }
        DateTimeOffset UpdatedAt { get; set; }
    }
}
