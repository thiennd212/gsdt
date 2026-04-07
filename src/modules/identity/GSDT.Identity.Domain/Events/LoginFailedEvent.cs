
namespace GSDT.Identity.Domain.Events;

/// <summary>Raised after failed login attempt — triggers lockout tracking in Audit module.</summary>
public sealed record LoginFailedEvent(
    string Email,
    string IpAddress,
    string Reason) : IInternalDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
