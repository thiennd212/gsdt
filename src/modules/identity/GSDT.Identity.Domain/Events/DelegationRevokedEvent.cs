
namespace GSDT.Identity.Domain.Events;

/// <summary>Raised when a role delegation is revoked — consumed by Audit module.</summary>
public sealed record DelegationRevokedEvent(
    Guid DelegationId,
    Guid DelegatorId,
    Guid DelegateId,
    Guid ActorId) : IInternalDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
