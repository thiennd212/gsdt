
namespace GSDT.Identity.Domain.Events;

/// <summary>Raised when admin locks or unlocks a user account.</summary>
public sealed record UserLockedEvent(
    Guid UserId,
    bool IsLocked,
    Guid LockedBy) : IInternalDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
