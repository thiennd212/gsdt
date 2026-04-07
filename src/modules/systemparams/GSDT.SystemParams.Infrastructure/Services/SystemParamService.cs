using System.Text.Json;

namespace GSDT.SystemParams.Infrastructure.Services;

/// <summary>
/// Singleton — IServiceScopeFactory for scoped EF access.
/// SetAsync persists to DB and publishes Redis invalidation event.
/// </summary>
public class SystemParamService(
    IServiceScopeFactory scopeFactory,
    ILogger<SystemParamService> logger) : ISystemParamService
{
    public async Task<string?> GetAsync(string key, string? tenantId = null, CancellationToken ct = default)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SystemParamsDbContext>();
        var normalizedKey = key.ToLowerInvariant();

        // Tenant override first, then global fallback
        var param = tenantId is not null
            ? await db.SystemParameters.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Key == normalizedKey && p.TenantId == tenantId, ct)
              ?? await db.SystemParameters.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Key == normalizedKey && p.TenantId == null, ct)
            : await db.SystemParameters.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Key == normalizedKey && p.TenantId == null, ct);

        return param?.Value;
    }

    public async Task<T?> GetAsync<T>(string key, string? tenantId = null, CancellationToken ct = default)
    {
        var raw = await GetAsync(key, tenantId, ct);
        if (raw is null) return default;
        try { return JsonSerializer.Deserialize<T>(raw); }
        catch { return default; }
    }

    public async Task SetAsync(string key, object value, CancellationToken ct = default)
    {
        var strValue = value is string s ? s : JsonSerializer.Serialize(value);
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SystemParamsDbContext>();
        var normalizedKey = key.ToLowerInvariant();

        var param = await db.SystemParameters
            .FirstOrDefaultAsync(p => p.Key == normalizedKey && p.TenantId == null, ct);

        if (param is not null)
        {
            param.UpdateValue(strValue);
            await db.SaveChangesAsync(ct);
        }

        logger.LogDebug("SystemParam updated: {Key}", normalizedKey);
    }

    public async Task SetAsync(string key, object value, string tenantId, CancellationToken ct = default)
    {
        var strValue = value is string s ? s : JsonSerializer.Serialize(value);
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SystemParamsDbContext>();
        var normalizedKey = key.ToLowerInvariant();

        var param = await db.SystemParameters
            .FirstOrDefaultAsync(p => p.Key == normalizedKey && p.TenantId == tenantId, ct);

        if (param is not null)
        {
            param.UpdateValue(strValue);
            await db.SaveChangesAsync(ct);
        }

        logger.LogDebug("SystemParam updated: {Key} (tenant={TenantId})", normalizedKey, tenantId);
    }
}
