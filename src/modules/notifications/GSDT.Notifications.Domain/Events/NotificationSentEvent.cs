
namespace GSDT.Notifications.Domain.Events;

/// <summary>
/// External event — dispatched via Outbox → MassTransit after notification is sent.
/// IDs only — no PII per outbox security spec.
/// </summary>
public sealed record NotificationSentEvent(
    Guid NotificationId,
    Guid TenantId,
    Guid RecipientUserId,
    string Channel) : IExternalDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
