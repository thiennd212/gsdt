
namespace GSDT.Audit.Domain.Entities;

/// <summary>Security incident record — QĐ742 mandated tracking.</summary>
public sealed class SecurityIncident
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid? TenantId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public AuditSeverity Severity { get; private set; }
    public IncidentStatus Status { get; private set; } = IncidentStatus.Open;
    public string Description { get; private set; } = string.Empty;
    public Guid ReportedBy { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public DateTimeOffset? ResolvedAt { get; private set; }
    public string? Mitigations { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    private SecurityIncident() { }

    public static SecurityIncident Report(
        Guid? tenantId,
        string title,
        AuditSeverity severity,
        string description,
        Guid reportedBy,
        DateTimeOffset occurredAt)
    {
        return new SecurityIncident
        {
            TenantId = tenantId,
            Title = title,
            Severity = severity,
            Description = description,
            ReportedBy = reportedBy,
            OccurredAt = occurredAt
        };
    }

    public void UpdateStatus(IncidentStatus status, string? mitigationNote)
    {
        Status = status;
        if (!string.IsNullOrEmpty(mitigationNote))
            Mitigations = (Mitigations ?? string.Empty) + $"\n[{DateTimeOffset.UtcNow:u}] {mitigationNote}";
        if (status is IncidentStatus.Resolved or IncidentStatus.Closed)
            ResolvedAt = DateTimeOffset.UtcNow;
    }
}
