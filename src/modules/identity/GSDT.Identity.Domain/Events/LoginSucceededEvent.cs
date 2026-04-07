
namespace GSDT.Identity.Domain.Events;

/// <summary>Raised after successful authentication — consumed by Audit module.</summary>
public sealed record LoginSucceededEvent(
    Guid UserId,
    string IpAddress,
    string ClientId) : IInternalDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
