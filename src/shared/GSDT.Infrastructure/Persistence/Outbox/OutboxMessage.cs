namespace GSDT.Infrastructure.Persistence.Outbox;

/// <summary>
/// Outbox message — persisted in the same DB transaction as the aggregate change.
/// Processed asynchronously by OutboxProcessor or MassTransit EF Outbox (Phase 02c).
/// IMPORTANT: Payload MUST NOT contain PII for IExternalDomainEvent — IDs only (ArchUnit Phase 10).
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Assembly-qualified type name of the domain event (for deserialization).</summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>JSON-serialized event payload. External events: IDs only, no PII.</summary>
    public string Payload { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Set when successfully dispatched. Null = pending processing.</summary>
    public DateTimeOffset? ProcessedAtUtc { get; set; }

    /// <summary>Last error message if processing failed.</summary>
    public string? Error { get; set; }

    public int RetryCount { get; set; }

    /// <summary>Module schema that owns this message (e.g. "notifications", "identity").</summary>
    public string SchemaName { get; set; } = string.Empty;
}
