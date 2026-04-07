using Hangfire;

namespace GSDT.Files.Infrastructure.Jobs;

/// <summary>
/// Hangfire job: enforces retention policies by finding expired records and
/// archiving them or marking them for destruction.
/// Runs daily at 02:00 (registered in InfrastructureRegistration).
/// Checks ArchiveAfterDays and DestroyAfterDays thresholds against file upload date.
/// [DisableConcurrentExecution] prevents overlapping runs during large tenant scans.
/// </summary>
[AutomaticRetry(Attempts = 0)]
[DisableConcurrentExecution(3600)]
public sealed class RetentionPolicyEnforcementJob(
    IReadDbConnection db,
    FilesDbContext ctx,
    ILogger<RetentionPolicyEnforcementJob> logger)
{
    // System actor for automated lifecycle transitions
    private static readonly Guid SystemActorId = Guid.Empty;

    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        await ArchiveExpiredRecordsAsync(ct);
        await MarkForDestructionAsync(ct);
    }

    /// <summary>Archive records that have exceeded ArchiveAfterDays threshold.</summary>
    private async Task ArchiveExpiredRecordsAsync(CancellationToken ct)
    {
        const string sql = """
            SELECT f.Id AS FileRecordId, p.ArchiveAfterDays, p.Id AS PolicyId
            FROM files.FileRecords f
            INNER JOIN files.RetentionPolicies p
                ON p.TenantId = f.TenantId AND p.IsActive = 1 AND p.DocumentType = f.ContentType
            LEFT JOIN files.RecordLifecycles l ON l.FileRecordId = f.Id
            WHERE f.IsDeleted = 0
              AND p.ArchiveAfterDays IS NOT NULL
              AND DATEDIFF(DAY, f.CreatedAt, GETUTCDATE()) >= p.ArchiveAfterDays
              AND (l.Id IS NULL OR l.CurrentStatus = 0)
            """;

        var records = (await db.QueryAsync<ExpiredRow>(sql, cancellationToken: ct)).ToList();

        if (records.Count == 0)
            return;

        logger.LogInformation("RetentionPolicyEnforcementJob: archiving {Count} records.", records.Count);

        foreach (var row in records)
        {
            // Check if lifecycle record already exists
            var lifecycle = await ctx.RecordLifecycles
                .FirstOrDefaultAsync(l => l.FileRecordId == row.FileRecordId, ct);

            if (lifecycle is null)
            {
                lifecycle = RecordLifecycle.Create(row.FileRecordId, row.PolicyId, SystemActorId);
                lifecycle.Archive(SystemActorId);
                await ctx.RecordLifecycles.AddAsync(lifecycle, ct);
            }
            else
            {
                lifecycle.Archive(SystemActorId);
            }
        }

        await ctx.SaveChangesAsync(ct);
        logger.LogInformation("RetentionPolicyEnforcementJob: archived {Count} records.", records.Count);
    }

    /// <summary>Mark archived records for destruction when DestroyAfterDays threshold reached.</summary>
    private async Task MarkForDestructionAsync(CancellationToken ct)
    {
        const string sql = """
            SELECT l.Id AS LifecycleId, l.FileRecordId,
                   p.DestroyAfterDays,
                   DATEDIFF(DAY, f.CreatedAt, GETUTCDATE()) AS AgeDays
            FROM files.RecordLifecycles l
            INNER JOIN files.FileRecords f ON f.Id = l.FileRecordId
            INNER JOIN files.RetentionPolicies p ON p.Id = l.RetentionPolicyId
            WHERE l.CurrentStatus = 1
              AND p.DestroyAfterDays IS NOT NULL
              AND DATEDIFF(DAY, f.CreatedAt, GETUTCDATE()) >= p.DestroyAfterDays
              AND l.ScheduledDestroyAt IS NULL
              AND f.IsDeleted = 0
            """;

        var records = (await db.QueryAsync<DestructionRow>(sql, cancellationToken: ct)).ToList();

        if (records.Count == 0)
            return;

        logger.LogInformation(
            "RetentionPolicyEnforcementJob: scheduling destruction for {Count} records.", records.Count);

        foreach (var row in records)
        {
            var lifecycle = await ctx.RecordLifecycles.FindAsync([row.LifecycleId], ct);
            lifecycle?.ScheduleDestruction(DateTime.UtcNow.AddDays(7), SystemActorId);
        }

        await ctx.SaveChangesAsync(ct);
    }

    private sealed record ExpiredRow(
        Guid FileRecordId, int ArchiveAfterDays, Guid PolicyId);

    private sealed record DestructionRow(
        Guid LifecycleId, Guid FileRecordId,
        int DestroyAfterDays, int AgeDays);
}
