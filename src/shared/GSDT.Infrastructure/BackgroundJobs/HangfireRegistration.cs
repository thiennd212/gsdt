using Hangfire;
using Hangfire.SqlServer;

namespace GSDT.Infrastructure.BackgroundJobs;

/// <summary>
/// Registers Hangfire with SQL Server storage (hangfire schema).
/// Queue priority order: webhook-critical → webhook-normal → audit → background → default.
/// Brainstorm decision: webhook 1s, audit 2s, background 15s polling intervals handled
/// by Hangfire's internal dispatcher per queue priority — no manual polling config needed.
/// </summary>
public static class HangfireRegistration
{
    public static IServiceCollection AddHangfireJobs(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connStr = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:Default is required for Hangfire SQL Server storage.");

        services.AddHangfire(cfg => cfg
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseFilter(new PiiMaskingJobFilter())
            .UseSqlServerStorage(connStr, new SqlServerStorageOptions
            {
                SchemaName = "hangfire",
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero, // use SQL Server long-polling (immediate)
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true           // avoids distributed lock table contention
            }));

        services.AddHangfireServer(opt =>
        {
            // Cap at 20 workers to prevent thread pool exhaustion under load
            opt.WorkerCount = Math.Min(Environment.ProcessorCount * 5, 20);
            opt.Queues = ["webhook-critical", "webhook-normal", "audit", "background", "default"];
            opt.SchedulePollingInterval = TimeSpan.FromSeconds(2);
            opt.ShutdownTimeout = TimeSpan.FromSeconds(30);
        });

        // Job abstractions — Scoped so job classes resolve Scoped services via DI
        services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();
        services.AddScoped<IArchiveService, SqlServerArchiveService>();

        return services;
    }

    /// <summary>
    /// Idempotent migration: renames old colon-delimited queue names to dash-delimited.
    /// Safe to call on every startup — only updates rows that still use old names.
    /// </summary>
    public static void MigrateHangfireQueues(IConfiguration configuration, ILogger? logger = null)
    {
        var connStr = configuration.GetConnectionString("Default");
        if (string.IsNullOrEmpty(connStr)) return;

        const string sql = """
            IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('hangfire.Job') AND name = 'Queue')
            BEGIN
                UPDATE hangfire.[Job]
                SET Queue = REPLACE(REPLACE(Queue, 'webhook:critical', 'webhook-critical'),
                                    'webhook:normal', 'webhook-normal')
                WHERE Queue IN ('webhook:critical', 'webhook:normal')
                  AND StateName IN ('Enqueued', 'Scheduled', 'Processing')
            END
            """;

        try
        {
            using var conn = new SqlConnection(connStr);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            var affected = cmd.ExecuteNonQuery();
            if (affected > 0)
                logger?.LogInformation("Hangfire queue migration: {Count} jobs requeued from old queue names", affected);
        }
        catch (Exception ex)
        {
            // Non-fatal: jobs will just stay in old queues until next startup
            logger?.LogWarning(ex, "Hangfire queue migration failed — will retry on next startup");
        }
    }

    /// <summary>
    /// Registers NĐ53 recurring archive jobs.
    /// Called after app.Build() when IApplicationBuilder is available and DI is sealed.
    /// Hangfire stores job definitions in SQL — safe to call on every startup (idempotent).
    /// </summary>
    public static void RegisterRecurringJobs()
    {
        // NĐ53: audit logs — 12-month hot retention, archive on 1st of each month at 03:00
        RecurringJob.AddOrUpdate<IArchiveService>(
            "archive-audit",
            queue: "background",
            methodCall: svc => svc.ArchiveRecordsOlderThanAsync(
                "audit", TimeSpan.FromDays(365), CancellationToken.None),
            cronExpression: "0 3 1 * *");

        // NĐ53: cases — 24-month hot retention
        RecurringJob.AddOrUpdate<IArchiveService>(
            "archive-cases",
            queue: "background",
            methodCall: svc => svc.ArchiveRecordsOlderThanAsync(
                "cases", TimeSpan.FromDays(730), CancellationToken.None),
            cronExpression: "0 3 1 * *");
    }
}
