
namespace GSDT.Files.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF config for RecordLifecycle.
/// One-to-one with FileRecord — unique constraint on FileRecordId enforces this.
/// ScheduledDestroyAt indexed for enforcement job to find records due for destruction.
/// CurrentStatus indexed for enforcement job to find active/archived records.
/// </summary>
public sealed class RecordLifecycleConfiguration
    : EntityTypeConfigurationBase<RecordLifecycle, Guid>
{
    protected override void ConfigureEntity(EntityTypeBuilder<RecordLifecycle> builder)
    {
        builder.ToTable("RecordLifecycles");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.FileRecordId).IsRequired();
        builder.Property(l => l.CurrentStatus).IsRequired();

        // One lifecycle record per file
        builder.HasIndex(l => l.FileRecordId)
            .IsUnique()
            .HasDatabaseName("UX_RecordLifecycles_FileRecordId");

        // Enforcement job: find records scheduled for destruction
        builder.HasIndex(l => new { l.CurrentStatus, l.ScheduledDestroyAt })
            .HasDatabaseName("IX_RecordLifecycles_Status_ScheduledDestroyAt");
    }
}
