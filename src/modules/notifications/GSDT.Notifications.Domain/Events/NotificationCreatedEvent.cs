
namespace GSDT.Notifications.Domain.Events;

/// <summary>
/// Internal event — dispatched via MediatR within same transaction.
/// Triggers notification delivery side-effects within the module boundary.
/// </summary>
public sealed record NotificationCreatedEvent(
    Guid NotificationId,
    Guid TenantId,
    Guid RecipientUserId,
    string Channel) : IInternalDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
