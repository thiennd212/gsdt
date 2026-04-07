
namespace GSDT.Files.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for FileRecord aggregate.
/// UploaderDisplayName encrypted at rest (AES-256-GCM) — PII field.
/// StorageKey indexed for ClamAV job lookups.
/// </summary>
public sealed class FileRecordConfiguration : IEntityTypeConfiguration<FileRecord>
{
    private readonly byte[]? _encryptionKey;

    // No parameterless ctor — forces manual registration in FilesDbContext.OnModelCreating
    // so IConfiguration can be injected for encryption key (C-04 security fix)
    public FileRecordConfiguration(IConfiguration? configuration)
    {
        var keyBase64 = configuration?["Encryption:FieldKey"];
        if (!string.IsNullOrEmpty(keyBase64))
        {
            try
            {
                _encryptionKey = Convert.FromBase64String(keyBase64);
                if (_encryptionKey.Length != 32)
                    throw new InvalidOperationException(
                        $"Encryption:FieldKey must be exactly 32 bytes (AES-256). Got {_encryptionKey.Length} bytes.");
            }
            catch (FormatException ex)
            {
                throw new InvalidOperationException(
                    "Encryption:FieldKey is not valid base64. Check appsettings or Vault configuration.", ex);
            }
        }
    }

    public void Configure(EntityTypeBuilder<FileRecord> builder)
    {
        builder.ToTable("FileRecords");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id)
            .HasConversion(id => id.Value, value => FileId.From(value));

        builder.Property(f => f.OriginalFileName).HasMaxLength(255).IsRequired();
        builder.Property(f => f.StorageKey).HasMaxLength(512).IsRequired();
        builder.Property(f => f.ContentType).HasMaxLength(127).IsRequired();
        builder.Property(f => f.ChecksumSha256).HasMaxLength(64).IsRequired();
        builder.Property(f => f.Status).IsRequired();
        builder.Property(f => f.SizeBytes).IsRequired();
        builder.Property(f => f.TenantId).IsRequired();
        builder.Property(f => f.UploadedBy).IsRequired();

        // PII field — encrypted at rest
        if (_encryptionKey is not null)
        {
            builder.Property(f => f.UploaderDisplayName)
                .HasMaxLength(1024) // encrypted value is longer than plaintext
                .HasConversion((Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter)new EncryptedStringConverter(_encryptionKey));
        }
        else
        {
            builder.Property(f => f.UploaderDisplayName).HasMaxLength(512);
        }

        builder.HasIndex(f => f.StorageKey).IsUnique();
        builder.HasIndex(f => new { f.TenantId, f.Status });
        builder.HasIndex(f => f.UploadedBy);
    }
}
