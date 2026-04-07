
namespace GSDT.Audit.Domain.Entities;

/// <summary>
/// NĐ53 / Law 91/2025: Personal Data Processing (PDP) log.
/// Records WHO processed WHAT PII, WHEN, for WHAT purpose, under WHAT legal basis.
/// </summary>
public sealed class PersonalDataProcessingLog
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid TenantId { get; private set; }
    [DataClassification(DataClassificationLevel.Internal)]
    public Guid ProcessorId { get; private set; }
    [DataClassification(DataClassificationLevel.Internal)]
    public Guid DataSubjectId { get; private set; }
    public string DataCategory { get; private set; } = string.Empty; // e.g. "CCCD", "Phone", "Address"
    public string Purpose { get; private set; } = string.Empty;
    public string LegalBasis { get; private set; } = string.Empty; // e.g. "consent", "legal_obligation"
    public string ProcessingAction { get; private set; } = string.Empty; // collect, use, share, store, delete
    public DateTimeOffset ProcessedAt { get; private set; } = DateTimeOffset.UtcNow;

    private PersonalDataProcessingLog() { }

    public static PersonalDataProcessingLog Record(
        Guid tenantId, Guid processorId, Guid dataSubjectId,
        string dataCategory, string purpose, string legalBasis, string processingAction) =>
        new()
        {
            TenantId = tenantId,
            ProcessorId = processorId,
            DataSubjectId = dataSubjectId,
            DataCategory = dataCategory,
            Purpose = purpose,
            LegalBasis = legalBasis,
            ProcessingAction = processingAction
        };
}
