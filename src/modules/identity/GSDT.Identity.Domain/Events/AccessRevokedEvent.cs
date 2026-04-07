
namespace GSDT.Identity.Domain.Events;

/// <summary>Raised when access review results in Revoke decision — Audit module logs it.</summary>
public sealed record AccessRevokedEvent(
    Guid UserId,
    Guid RoleId,
    Guid ReviewedBy,
    Guid AccessReviewRecordId) : IInternalDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
