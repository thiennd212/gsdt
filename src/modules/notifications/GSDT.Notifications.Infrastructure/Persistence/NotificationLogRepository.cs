
namespace GSDT.Notifications.Infrastructure.Persistence;

public sealed class NotificationLogRepository(NotificationsDbContext dbContext) : INotificationLogRepository
{
    public async Task<bool> ExistsAsync(
        Guid templateId, Guid recipientId, string correlationId, CancellationToken cancellationToken = default)
    {
        return await dbContext.NotificationLogs
            .AnyAsync(l =>
                l.TemplateId == templateId &&
                l.RecipientId == recipientId &&
                l.CorrelationId == correlationId,
                cancellationToken);
    }

    public async Task AddAsync(NotificationLog log, CancellationToken cancellationToken = default)
    {
        await dbContext.NotificationLogs.AddAsync(log, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
