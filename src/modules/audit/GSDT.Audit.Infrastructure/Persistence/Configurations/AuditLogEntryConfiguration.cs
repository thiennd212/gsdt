
namespace GSDT.Audit.Infrastructure.Persistence.Configurations;

public sealed class AuditLogEntryConfiguration : IEntityTypeConfiguration<AuditLogEntry>
{
    public void Configure(EntityTypeBuilder<AuditLogEntry> b)
    {
        b.ToTable("AuditLogEntries");
        b.HasKey(e => e.Id);

        b.Property(e => e.UserName).HasMaxLength(256).IsRequired();
        b.Property(e => e.Action).HasMaxLength(64).IsRequired();
        b.Property(e => e.ModuleName).HasMaxLength(128).IsRequired();
        b.Property(e => e.ResourceType).HasMaxLength(128).IsRequired();
        b.Property(e => e.ResourceId).HasMaxLength(256);
        b.Property(e => e.IpAddress).HasMaxLength(64);
        b.Property(e => e.CorrelationId).HasMaxLength(128);
        b.Property(e => e.HmacSignature).HasMaxLength(128).IsRequired();
        b.Property(e => e.DataSnapshot).HasColumnType("nvarchar(max)");

        // SequenceId is DB-assigned via identity column for guaranteed ordering
        b.Property(e => e.SequenceId).UseIdentityColumn();

        // Indexes for common filter patterns
        b.HasIndex(e => new { e.TenantId, e.OccurredAt });
        b.HasIndex(e => new { e.UserId, e.OccurredAt });
        b.HasIndex(e => e.Action);
        b.HasIndex(e => e.ModuleName);
    }
}
