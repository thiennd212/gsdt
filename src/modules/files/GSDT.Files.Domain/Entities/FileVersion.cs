
namespace GSDT.Files.Domain.Entities;

/// <summary>
/// Represents a specific version of a FileRecord.
/// VersionNumber increments monotonically per FileRecord.
/// ContentHash (SHA-256) enables deduplication and integrity verification.
/// StoragePath is the physical path in object storage for this version.
/// </summary>
public sealed class FileVersion : AuditableEntity<Guid>, ITenantScoped
{
    public Guid FileRecordId { get; private set; }
    public int VersionNumber { get; private set; }
    public string StoragePath { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public string ContentHash { get; private set; } = string.Empty;
    public Guid UploadedBy { get; private set; }
    public DateTime UploadedAt { get; private set; }
    public string? Comment { get; private set; }
    public Guid TenantId { get; private set; }

    private FileVersion() { } // EF Core

    public static FileVersion Create(
        Guid fileRecordId,
        int versionNumber,
        string storagePath,
        long fileSize,
        string contentHash,
        Guid uploadedBy,
        Guid tenantId,
        string? comment = null)
    {
        var version = new FileVersion
        {
            Id = Guid.NewGuid(),
            FileRecordId = fileRecordId,
            VersionNumber = versionNumber,
            StoragePath = storagePath,
            FileSize = fileSize,
            ContentHash = contentHash,
            UploadedBy = uploadedBy,
            UploadedAt = DateTime.UtcNow,
            Comment = comment,
            TenantId = tenantId
        };
        version.SetAuditCreate(uploadedBy);
        return version;
    }
}
