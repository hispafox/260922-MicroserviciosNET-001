using Notifications.Infrastructure.Persistence;
using Notifications.Domain.Entities;

namespace Notifications.Application;
public class NotificationsService
{
    private readonly NotificationsDbContext _db;
    public NotificationsService(NotificationsDbContext db) => _db = db;
    public async Task AddAsync(Guid orderId, string type, string message, CancellationToken ct)
    {
        await _db.Notifications.AddAsync(new Notification { OrderId = orderId, Type = type, Message = message }, ct);
        await _db.SaveChangesAsync(ct);
    }
}
