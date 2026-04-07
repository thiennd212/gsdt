
namespace GSDT.Identity.Domain.Entities;

/// <summary>
/// PDPL consent record per Law 91/2025/QH15 + Decree 356/2025.
/// Tracks data processing consent, legal basis, and withdrawal.
/// </summary>
public class ConsentRecord
{
    public Guid Id { get; set; }

    /// <summary>Data subject (citizen/officer) who gave consent.</summary>
    [DataClassification(DataClassificationLevel.Internal)]
    public Guid DataSubjectId { get; set; }

    /// <summary>Purpose of data processing, e.g. "case_management", "audit_log".</summary>
    public string Purpose { get; set; } = string.Empty;

    /// <summary>Legal basis per Law 91/2025: Consent | LegalObligation | VitalInterest | PublicTask | LegitimateInterest.</summary>
    public string LegalBasis { get; set; } = string.Empty;

    /// <summary>Type of data subject: "citizen" | "officer" | "organization".</summary>
    public string? DataSubjectType { get; set; }

    /// <summary>JSON evidence: IP, UserAgent, form context, etc. for audit trail.</summary>
    public string? EvidenceJson { get; set; }

    public bool IsWithdrawn { get; set; }
    public DateTime? WithdrawnAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public Guid? TenantId { get; set; }
}
