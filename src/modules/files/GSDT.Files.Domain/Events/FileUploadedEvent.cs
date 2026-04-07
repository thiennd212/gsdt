
namespace GSDT.Files.Domain.Events;

/// <summary>Raised after ClamAV scan passes — file status becomes Available.</summary>
public sealed record FileUploadedEvent(
    FileId FileId,
    Guid TenantId,
    string OriginalFileName,
    long SizeBytes) : IInternalDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
