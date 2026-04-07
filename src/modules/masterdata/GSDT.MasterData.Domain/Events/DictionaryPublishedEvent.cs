
namespace GSDT.MasterData.Domain.Events;

/// <summary>
/// External domain event — published via MassTransit after a Dictionary is successfully published.
/// Consumers (e.g. cache invalidator) react to this to purge stale dictionary data.
/// Contains IDs only — no PII (IExternalDomainEvent contract).
/// </summary>
public sealed record DictionaryPublishedEvent(
    Guid DictionaryId,
    string DictionaryCode,
    Guid TenantId,
    int VersionNumber,
    int ItemCount) : IExternalDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
