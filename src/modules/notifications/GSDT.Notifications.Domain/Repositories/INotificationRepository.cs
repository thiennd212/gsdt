
namespace GSDT.Notifications.Domain.Repositories;

/// <summary>Per-aggregate repository interface — extends generic IRepository{Notification, Guid}.</summary>
public interface INotificationRepository : IRepository<Notification, Guid>
{
    Task<IReadOnlyList<Notification>> GetUnreadByUserAsync(
        Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
}
