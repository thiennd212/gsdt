
namespace GSDT.Infrastructure.Alerting;

/// <summary>
/// Minimal DbContext for alerting entities — schema: alerting.
/// Uses plain DbContext (not ModuleDbContext) — alerting is cross-cutting infrastructure,
/// not a tenant-scoped module, so global tenant/soft-delete filters are intentionally omitted.
/// AlertRules are system-wide; soft-delete is handled via Enabled flag instead.
/// </summary>
public sealed class AlertingDbContext : DbContext
{
    public AlertingDbContext(DbContextOptions<AlertingDbContext> options) : base(options) { }

public DbSet<AlertRule> AlertRules => Set<AlertRule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("alerting");

        modelBuilder.Entity<AlertRule>(entity =>
        {
            entity.ToTable("AlertRules");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.MetricName).HasMaxLength(300).IsRequired();
            entity.Property(e => e.Condition).HasMaxLength(10).IsRequired();
            entity.Property(e => e.NotifyChannel).HasMaxLength(50).IsRequired();
            entity.Property(e => e.NotifyTarget).HasMaxLength(500);

            // Audit columns from AuditableEntity<Guid>
            entity.Property(e => e.CreatedBy).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasIndex(e => e.Enabled);
            entity.HasIndex(e => e.MetricName);

            // Exclude soft-delete navigation — alerting uses Enabled flag only
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

    }
}
