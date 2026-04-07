
namespace GSDT.Infrastructure.Backup;

/// <summary>Minimal DbContext for backup records — schema: backup.</summary>
public sealed class BackupDbContext : DbContext
{
    public BackupDbContext(DbContextOptions<BackupDbContext> options) : base(options) { }

    public DbSet<BackupRecord> BackupRecords => Set<BackupRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("backup");

        modelBuilder.Entity<BackupRecord>(entity =>
        {
            entity.ToTable("BackupRecords");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            entity.HasIndex(e => e.StartedAt).IsDescending();
        });
    }
}
