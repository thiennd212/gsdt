
namespace GSDT.Files.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF config for FileVersion.
/// Unique constraint on (FileRecordId, VersionNumber) prevents duplicate version numbers per file.
/// ContentHash indexed for deduplication checks across versions.
/// </summary>
public sealed class FileVersionConfiguration
    : EntityTypeConfigurationBase<FileVersion, Guid>
{
    protected override void ConfigureEntity(EntityTypeBuilder<FileVersion> builder)
    {
        builder.ToTable("FileVersions");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.FileRecordId).IsRequired();
        builder.Property(v => v.VersionNumber).IsRequired();
        builder.Property(v => v.StoragePath).HasMaxLength(1024).IsRequired();
        builder.Property(v => v.FileSize).IsRequired();
        builder.Property(v => v.ContentHash).HasMaxLength(64).IsRequired();
        builder.Property(v => v.UploadedBy).IsRequired();
        builder.Property(v => v.UploadedAt).IsRequired();
        builder.Property(v => v.Comment).HasMaxLength(500);
        builder.Property(v => v.TenantId).IsRequired();

        // Version numbers must be unique per file record
        builder.HasIndex(v => new { v.FileRecordId, v.VersionNumber })
            .IsUnique()
            .HasDatabaseName("UX_FileVersions_FileRecordId_VersionNumber");

        // Deduplication: find existing version by content hash within tenant
        builder.HasIndex(v => new { v.TenantId, v.ContentHash })
            .HasDatabaseName("IX_FileVersions_TenantId_ContentHash");
    }
}
