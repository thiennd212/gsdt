using MediatR;

namespace GSDT.Notifications.Application.EventHandlers;

/// <summary>
/// Example cross-module event handler: listens to CaseCreatedIntegrationEvent from Cases module.
/// Demonstrates the pattern: integration event → INotificationModuleClient → in-app notification.
///
/// NOTE: In the monolith, this is wired via MassTransit consumer.
/// The Cases module raises IExternalDomainEvent → Outbox → MassTransit → this consumer.
/// Implements INotification so it can also be dispatched in-process via MediatR for testing.
/// </summary>
public sealed record CaseCreatedIntegrationEvent(
    Guid CaseId,
    Guid TenantId,
    Guid SubmittedByUserId,
    string CaseNumber) : INotification;

public sealed class NotifyOnCaseCreatedHandler(
    INotificationModuleClient notificationClient,
    IFeatureFlagService featureFlags,
    ILogger<NotifyOnCaseCreatedHandler> logger)
    : INotificationHandler<CaseCreatedIntegrationEvent>
{
    public async Task Handle(CaseCreatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        // In-app notification always sent
        await notificationClient.SendAsync(new SendNotificationRequest(
            RecipientUserId: notification.SubmittedByUserId,
            TenantId: notification.TenantId,
            Subject: "Hồ sơ đã được tiếp nhận",
            Body: $"Hồ sơ {notification.CaseNumber} của bạn đã được tiếp nhận thành công.",
            Channel: "inapp",
            CorrelationId: notification.CaseId.ToString()),
            cancellationToken);

        // SMS notification gated by feature flag
        if (featureFlags.IsEnabled("feature:notifications.sms", notification.TenantId.ToString()))
        {
            await notificationClient.SendAsync(new SendNotificationRequest(
                RecipientUserId: notification.SubmittedByUserId,
                TenantId: notification.TenantId,
                Subject: "Hồ sơ đã được tiếp nhận",
                Body: $"Hồ sơ {notification.CaseNumber} đã được tiếp nhận.",
                Channel: "sms",
                CorrelationId: notification.CaseId.ToString()),
                cancellationToken);
        }
        else
        {
            logger.LogInformation(
                "SMS disabled by feature flag for Case {CaseNumber}", notification.CaseNumber);
        }
    }
}
