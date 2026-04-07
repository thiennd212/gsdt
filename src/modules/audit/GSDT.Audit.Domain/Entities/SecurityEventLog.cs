
namespace GSDT.Audit.Domain.Entities;

/// <summary>NĐ53 security investigation log — records security-relevant system events.</summary>
public sealed class SecurityEventLog
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid? TenantId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string? Source { get; private set; }
    [DataClassification(DataClassificationLevel.Confidential)]
    public string? IpAddress { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; } = DateTimeOffset.UtcNow;
    public bool IsSecurityRelevant { get; private set; } = true;

    private SecurityEventLog() { }

    public static SecurityEventLog Create(
        Guid? tenantId, string eventType, string description,
        string? source = null, string? ipAddress = null) =>
        new()
        {
            TenantId = tenantId,
            EventType = eventType,
            Description = description,
            Source = source,
            IpAddress = ipAddress
        };
}
