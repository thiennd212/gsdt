
namespace GSDT.Files.Domain.Entities;

/// <summary>
/// FileRecord aggregate root — represents a stored file with virus scan lifecycle.
/// Two-phase upload: Quarantined (uploaded) → Available (scan pass) | Rejected (scan fail).
/// Storage key is UUID-based (never user-supplied filename) — prevents path traversal.
/// </summary>
public sealed class FileRecord : AuditableEntity<FileId>, IAggregateRoot, ITenantScoped
{

    public Guid TenantId { get; private set; }

    /// <summary>Original user-supplied filename — stored in metadata only, never used as storage key.</summary>
    public string OriginalFileName { get; private set; } = string.Empty;

    /// <summary>UUID-based storage key: {tenantId}/{uuid}.{safeExt} — prevents path traversal.</summary>
    public string StorageKey { get; private set; } = string.Empty;

    public string ContentType { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }
    public string ChecksumSha256 { get; private set; } = string.Empty;
    public FileStatus Status { get; private set; }
    [DataClassification(DataClassificationLevel.Internal)]
    public Guid UploadedBy { get; private set; }
    public Guid? CaseId { get; private set; }

    /// <summary>Uploader full name — PII, encrypted at rest via EncryptedStringConverter.</summary>
    [DataClassification(DataClassificationLevel.Confidential)]
    public string? UploaderDisplayName { get; private set; }


    private FileRecord() { } // EF Core

    public static FileRecord Create(
        Guid tenantId,
        string originalFileName,
        string storageKey,
        string contentType,
        long sizeBytes,
        string checksumSha256,
        Guid uploadedBy,
        Guid? caseId = null,
        string? uploaderDisplayName = null,
        ClassificationLevel classification = ClassificationLevel.Internal)
    {
        var record = new FileRecord
        {
            Id = FileId.New(),
            TenantId = tenantId,
            OriginalFileName = originalFileName,
            StorageKey = storageKey,
            ContentType = contentType,
            SizeBytes = sizeBytes,
            ChecksumSha256 = checksumSha256,
            Status = FileStatus.Quarantined,
            UploadedBy = uploadedBy,
            CaseId = caseId,
            UploaderDisplayName = uploaderDisplayName,
            ClassificationLevel = classification
        };
        record.SetAuditCreate(uploadedBy);
        return record;
    }

    /// <summary>Called by ClamAvScanJob after clean scan — file becomes accessible.</summary>
    public void MarkAvailable()
    {
        Status = FileStatus.Available;
        MarkUpdated();
        AddDomainEvent(new FileUploadedEvent(Id, TenantId, OriginalFileName, SizeBytes));
    }

    /// <summary>Called by ClamAvScanJob when virus detected — file must be deleted from storage.</summary>
    public void MarkRejected()
    {
        Status = FileStatus.Rejected;
        MarkUpdated();
        AddDomainEvent(new FileQuarantinedEvent(Id, TenantId, OriginalFileName));
    }

    /// <summary>Soft delete — removes logical access but preserves audit trail.</summary>
    public new void Delete()
    {
        MarkDeleted();
        MarkUpdated();
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}

/// <summary>Two-phase virus scan lifecycle for file records.</summary>
public enum FileStatus
{
    /// <summary>Uploaded to MinIO, pending ClamAV scan. NOT accessible for download.</summary>
    Quarantined = 0,

    /// <summary>ClamAV scan passed — file is accessible for download.</summary>
    Available = 1,

    /// <summary>ClamAV detected virus — file deleted from MinIO, metadata retained for audit.</summary>
    Rejected = 2
}
