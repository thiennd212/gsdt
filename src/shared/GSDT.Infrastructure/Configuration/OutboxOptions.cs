namespace GSDT.Infrastructure.Configuration;

/// <summary>
/// Outbox processor configuration.
/// Phase 02c (MassTransit EF Outbox) overrides production dispatch — this configures the fallback processor.
/// </summary>
public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    /// <summary>Polling interval in seconds between outbox processing batches.</summary>
    public int PollingIntervalSeconds { get; set; } = 5;

    /// <summary>Maximum retries before marking a message as permanently failed.</summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>Maximum messages processed per polling cycle.</summary>
    public int BatchSize { get; set; } = 50;
}
