
namespace GSDT.Audit.Domain.Entities;

/// <summary>
/// Right to be Forgotten (RTBF) request — Law 91/2025 Art. 9 compliance.
/// Tracks status of PII anonymization pipeline execution across Identity/Cases/Forms/Audit modules.
/// </summary>
public sealed class RtbfRequest
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid TenantId { get; private set; }
    /// <summary>The user whose data is to be anonymized.</summary>
    [DataClassification(DataClassificationLevel.Internal)]
    public Guid DataSubjectId { get; private set; }
    /// <summary>
    /// Optional national ID (CCCD/CMND) for locating guest form submissions
    /// where SubmittedBy is Guid.Empty. Required when data subject submitted forms
    /// without an authenticated account.
    /// </summary>
    [DataClassification(DataClassificationLevel.Restricted)]
    public string? CitizenNationalId { get; private set; }
    /// <summary>Denormalized from Identity — populated at creation for search capability.</summary>
    [DataClassification(DataClassificationLevel.Confidential)]
    [MaxLength(256)]
    public string? SubjectEmail { get; private set; }
    public DateTimeOffset RequestedAt { get; private set; } = DateTimeOffset.UtcNow;
    /// <summary>SLA deadline — default 30 days from RequestedAt per agency policy.</summary>
    public DateTimeOffset DueBy { get; private set; }
    public RtbfStatus Status { get; private set; } = RtbfStatus.Pending;
    public Guid? ProcessedBy { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public string? RejectionReason { get; private set; }
    /// <summary>
    /// JSON log of per-module anonymization results when status is PartiallyCompleted.
    /// Allows safe idempotent retry — completed modules are skipped on re-run.
    /// </summary>
    public string? FailureLog { get; private set; }

    private RtbfRequest() { }

    public static RtbfRequest Create(Guid tenantId, Guid dataSubjectId, string? citizenNationalId = null, string? subjectEmail = null) =>
        new()
        {
            TenantId = tenantId,
            DataSubjectId = dataSubjectId,
            SubjectEmail = subjectEmail,
            CitizenNationalId = citizenNationalId,
            DueBy = DateTimeOffset.UtcNow.AddDays(30)
        };

    public void StartProcessing() => Status = RtbfStatus.Processing;

    public void Complete(Guid processedBy)
    {
        Status = RtbfStatus.Completed;
        ProcessedBy = processedBy;
        ProcessedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Mark partially complete when some modules succeeded and others failed.
    /// failureLog is a human-readable summary of which steps failed and why.
    /// Job is idempotent — operators can safely re-run to complete remaining modules.
    /// </summary>
    public void PartiallyComplete(Guid processedBy, string failureLog)
    {
        Status = RtbfStatus.PartiallyCompleted;
        ProcessedBy = processedBy;
        ProcessedAt = DateTimeOffset.UtcNow;
        FailureLog = failureLog;
    }

    public void Reject(Guid processedBy, string reason)
    {
        Status = RtbfStatus.Rejected;
        ProcessedBy = processedBy;
        ProcessedAt = DateTimeOffset.UtcNow;
        RejectionReason = reason;
    }
}
