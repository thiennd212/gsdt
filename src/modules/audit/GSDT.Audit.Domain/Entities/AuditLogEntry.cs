
namespace GSDT.Audit.Domain.Entities;

/// <summary>
/// HMAC-chained audit log entry — tamper-resistant, append-only.
/// HMAC chain: HmacSignature = HMAC(PreviousHash + EntityJson).
/// NĐ53: 12-month hot retention, archive after.
/// Law 91/2025: logs WHO accessed WHAT PII, WHEN, for WHAT purpose.
/// </summary>
public sealed class AuditLogEntry
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid? TenantId { get; private set; }
    [DataClassification(DataClassificationLevel.Internal)]
    public Guid? UserId { get; private set; }
    [DataClassification(DataClassificationLevel.Confidential)]
    public string UserName { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string ModuleName { get; private set; } = string.Empty;
    public string ResourceType { get; private set; } = string.Empty;
    public string? ResourceId { get; private set; }
    /// <summary>JSON snapshot of changed data — no raw PII, only IDs and non-sensitive fields.</summary>
    public string? DataSnapshot { get; private set; }
    [DataClassification(DataClassificationLevel.Confidential)]
    public string? IpAddress { get; private set; }
    [DataClassification(DataClassificationLevel.Confidential)]
    public string? UserAgent { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; } = DateTimeOffset.UtcNow;
    /// <summary>HMAC of (PreviousHash + current entry data) — tamper detection.</summary>
    public string HmacSignature { get; private set; } = string.Empty;
    public string? CorrelationId { get; private set; }
    public long SequenceId { get; private set; }

    private AuditLogEntry() { }

    public static AuditLogEntry Create(
        Guid? tenantId,
        Guid? userId,
        string userName,
        string action,
        string moduleName,
        string resourceType,
        string? resourceId,
        string? dataSnapshot,
        string? ipAddress,
        string? correlationId)
    {
        return new AuditLogEntry
        {
            TenantId = tenantId,
            UserId = userId,
            UserName = userName,
            Action = action,
            ModuleName = moduleName,
            ResourceType = resourceType,
            ResourceId = resourceId,
            DataSnapshot = dataSnapshot,
            IpAddress = ipAddress,
            CorrelationId = correlationId,
            OccurredAt = DateTimeOffset.UtcNow,
            HmacSignature = string.Empty // set by HmacAuditService after chain computation
        };
    }

    public void SetHmacSignature(string signature) => HmacSignature = signature;
    public void SetSequenceId(long id) => SequenceId = id;
}
