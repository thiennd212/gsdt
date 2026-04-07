
namespace GSDT.Audit.Infrastructure.Persistence;

/// <summary>
/// Audit module DB context — append-only enforcement.
/// Overrides SaveChanges to throw on Modified/Deleted entries (tamper prevention).
/// Does NOT inherit ModuleDbContext soft-delete filter — audit logs are never soft-deleted.
/// Schema: "audit".
/// </summary>
public sealed class AuditDbContext(DbContextOptions<AuditDbContext> options) : DbContext(options)
{
    public DbSet<AuditLogEntry> AuditLogEntries { get; set; } = default!;
    public DbSet<LoginAttempt> LoginAttempts { get; set; } = default!;
    public DbSet<PersonalDataProcessingLog> PersonalDataProcessingLogs { get; set; } = default!;
    public DbSet<SecurityEventLog> SecurityEventLogs { get; set; } = default!;
    public DbSet<SecurityIncident> SecurityIncidents { get; set; } = default!;
    public DbSet<RtbfRequest> RtbfRequests { get; set; } = default!;

    // M15 AI Governance
    public DbSet<AiPromptTrace> AiPromptTraces { get; set; } = default!;
    public DbSet<AiOutputReview> AiOutputReviews { get; set; } = default!;
    public DbSet<CompliancePolicy> CompliancePolicies { get; set; } = default!;
    public DbSet<CompliancePolicyEvaluation> CompliancePolicyEvaluations { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("audit");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuditDbContext).Assembly);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ThrowIfMutating();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ThrowIfMutating();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ThrowIfMutating()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State is EntityState.Modified or EntityState.Deleted
                && IsImmutableAuditEntity(entry))
            {
                throw new InvalidOperationException(
                    $"Audit entity '{entry.Metadata.Name}' is append-only. " +
                    $"Update and Delete are prohibited (tamper prevention).");
            }
        }
    }

    private static bool IsImmutableAuditEntity(EntityEntry entry) =>
        entry.Entity is AuditLogEntry or LoginAttempt
            or PersonalDataProcessingLog or SecurityEventLog
            or AiPromptTrace or CompliancePolicyEvaluation;
}
