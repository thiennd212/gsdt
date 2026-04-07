
namespace GSDT.SystemParams.Infrastructure.Services;

/// <summary>Seeds 5 default system params + 3 feature flags at startup (idempotent).</summary>
public class SystemParamSeeder(
    IServiceScopeFactory scopeFactory,
    ILogger<SystemParamSeeder> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SystemParamsDbContext>();

        await db.Database.EnsureCreatedAsync(ct);

        if (await db.SystemParameters.AnyAsync(ct))
        {
            logger.LogDebug("SystemParams already seeded — skipping");
            return;
        }

        var defaults = new[]
        {
            SystemParameter.Create("app:name",              "GSDT",     SystemParamDataType.String, "Application name",            false),
            SystemParameter.Create("app:version",           "1.0.0",         SystemParamDataType.String, "Application version",         false),
            SystemParameter.Create("app:max_upload_mb",     "100",           SystemParamDataType.Int,    "Max file upload in MB",       true),
            SystemParameter.Create("feature:audit_log",     "true",          SystemParamDataType.Bool,   "Enable audit logging",        true),
            SystemParameter.Create("feature:notifications", "true",          SystemParamDataType.Bool,   "Enable notifications",        true),
            SystemParameter.Create("feature:ai_assistant",  "false",         SystemParamDataType.Bool,   "Enable AI assistant",         true),
            SystemParameter.Create("feature:e_signature",   "false",         SystemParamDataType.Bool,   "Enable e-signature module",   true),
            SystemParameter.Create("session:timeout_min",   "60",            SystemParamDataType.Int,    "Session timeout in minutes",  true),
        };

        await db.SystemParameters.AddRangeAsync(defaults, ct);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("SystemParams seeded: {Count} defaults", defaults.Length);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
