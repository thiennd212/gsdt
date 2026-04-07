
namespace GSDT.Notifications.Domain.Repositories;

/// <summary>Repository for NotificationTemplate aggregate.</summary>
public interface INotificationTemplateRepository : IRepository<NotificationTemplate, Guid>
{
    Task<NotificationTemplate?> FindByKeyAsync(
        string templateKey, string channel, Guid tenantId, CancellationToken cancellationToken = default);
}
