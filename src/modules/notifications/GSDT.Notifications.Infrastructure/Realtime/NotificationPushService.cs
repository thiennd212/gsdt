using MediatR;

namespace GSDT.Notifications.Infrastructure.Realtime;

/// <summary>
/// Handles NotificationCreatedEvent (internal domain event) and pushes to SignalR.
/// Group targeting MANDATORY: always Clients.Group("user:{userId}"), never Clients.All.
/// </summary>
public sealed class NotificationPushService(
    IHubContext<NotificationsHub> hubContext,
    ILogger<NotificationPushService> logger)
    : INotificationHandler<NotificationCreatedEvent>
{
    public async Task Handle(NotificationCreatedEvent notification, CancellationToken cancellationToken)
    {
        var groupName = $"user:{notification.RecipientUserId}";

        await hubContext.Clients.Group(groupName).SendAsync(
            "NotificationCreated",
            new
            {
                notification.NotificationId,
                notification.TenantId,
                notification.RecipientUserId,
                notification.Channel,
                notification.OccurredAt
            },
            cancellationToken);

        logger.LogDebug(
            "SignalR push to group {Group} for notification {NotificationId}",
            groupName, notification.NotificationId);
    }
}
