
namespace GSDT.SystemParams;

/// <summary>
/// Seeds built-in system parameters and feature flags at startup (idempotent).
/// Runs synchronously in StartAsync so app is fully configured before first request.
/// </summary>
public class SystemParamSeeder(
    IServiceScopeFactory scopeFactory,
    ILogger<SystemParamSeeder> logger) : IHostedService
{
    // Built-in system parameters
    private static readonly (string Key, string Value, SystemParamDataType Type, string Desc)[] DefaultParams =
    [
        ("max_file_size_mb",        "10",                                              SystemParamDataType.Int,  "Maximum upload file size in megabytes"),
        ("allowed_file_extensions", "[\".pdf\",\".docx\",\".xlsx\",\".jpg\",\".png\"]", SystemParamDataType.Json, "Allowed file upload extensions"),
        ("session_timeout_minutes", "30",                                              SystemParamDataType.Int,  "User session timeout in minutes"),
        ("max_login_attempts",      "5",                                               SystemParamDataType.Int,  "Max failed login attempts before lockout"),
        ("maintenance_mode",        "false",                                           SystemParamDataType.Bool, "Enable maintenance mode (blocks non-admin requests)"),
    ];

    // Built-in feature flags (prefix: feature:)
    private static readonly (string Key, string Desc)[] DefaultFlags =
    [
        ("feature:dark_mode",           "Enable dark mode UI for all users"),
        ("feature:beta_api",            "Enable beta API endpoints (v2 preview)"),
        ("feature:maintenance_bypass",  "Allow bypassing maintenance mode for specific users"),
        ("feature:notifications.sms",   "Enable SMS notification channel (disable to log instead)"),
        ("feature:files.virus-scan",    "Enable ClamAV virus scan (disable to skip in dev/demo)"),
        ("feature:audit.hmac-chain",    "Enable HMAC chain verification on audit logs"),
        ("feature:cases.full-text-search", "Enable SQL Server FTS for case search (disable for LIKE fallback)"),
    ];

    public async Task StartAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SystemParamsDbContext>();

        await db.Database.EnsureCreatedAsync(ct);

        var seeded = 0;
        foreach (var (key, value, type, desc) in DefaultParams)
        {
            var exists = await db.SystemParameters.AnyAsync(p => p.Key == key && p.TenantId == null, ct);
            if (!exists)
            {
                db.SystemParameters.Add(SystemParameter.Create(key, value, type, desc, isEditable: true));
                seeded++;
            }
        }

        // Operational flags default to true (enabled) so existing behavior is unchanged
        var enabledByDefault = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "feature:notifications.sms",
            "feature:files.virus-scan",
            "feature:audit.hmac-chain",
            "feature:cases.full-text-search",
        };

        foreach (var (key, desc) in DefaultFlags)
        {
            var exists = await db.SystemParameters.AnyAsync(p => p.Key == key && p.TenantId == null, ct);
            if (!exists)
            {
                var defaultValue = enabledByDefault.Contains(key) ? "true" : "false";
                db.SystemParameters.Add(SystemParameter.Create(key, defaultValue, SystemParamDataType.Bool, desc, isEditable: true));
                seeded++;
            }
        }

        if (seeded > 0)
        {
            await db.SaveChangesAsync(ct);
            logger.LogInformation("SystemParamSeeder: seeded {Count} parameters", seeded);
        }
        else
        {
            logger.LogDebug("SystemParamSeeder: all parameters already present — skipping");
        }
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
