
namespace GSDT.Notifications.Domain.Repositories;

/// <summary>Idempotency log repository — guards against double-send on retries.</summary>
public interface INotificationLogRepository
{
    Task<bool> ExistsAsync(Guid templateId, Guid recipientId, string correlationId, CancellationToken cancellationToken = default);
    Task AddAsync(NotificationLog log, CancellationToken cancellationToken = default);
}
