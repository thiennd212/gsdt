
namespace GSDT.Files.Infrastructure.Persistence;

/// <summary>
/// Files module DB context — owns the "files" schema.
/// FileRecord.UploaderDisplayName is PII-encrypted via EncryptedStringConverter.
/// Manually applies FileRecordConfiguration with IConfiguration so encryption key is injected (C-04 fix).
/// </summary>
public sealed class FilesDbContext : ModuleDbContext
{
    private readonly IConfiguration _configuration;

    public FilesDbContext(
        DbContextOptions<FilesDbContext> options,
        ITenantContext tenantContext,
        IConfiguration configuration)
        : base(options, tenantContext)
    {
        _configuration = configuration;
    }

    protected override string SchemaName => "files";

    public DbSet<FileRecord> FileRecords { get; set; } = default!;

    // M08: Document lifecycle entities
    public DbSet<FileVersion> FileVersions { get; set; } = default!;
    public DbSet<DocumentTemplate> DocumentTemplates { get; set; } = default!;
    public DbSet<RetentionPolicy> RetentionPolicies { get; set; } = default!;
    public DbSet<RecordLifecycle> RecordLifecycles { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Base applies schema, soft-delete filter, outbox, and assembly-scanned configs.
        // FileRecordConfiguration has no parameterless ctor so it's skipped by assembly scan.
        base.OnModelCreating(modelBuilder);

        // Manually apply with IConfiguration so encryption key is available
        modelBuilder.ApplyConfiguration(new FileRecordConfiguration(_configuration));
    }
}
