
namespace GSDT.Files.Domain.Events;

/// <summary>Raised when ClamAV detects a virus — file status becomes Rejected.</summary>
public sealed record FileQuarantinedEvent(
    FileId FileId,
    Guid TenantId,
    string OriginalFileName) : IInternalDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
