using System.Text.Json;
using StackExchange.Redis;

namespace GSDT.SystemParams.Services;

/// <summary>
/// Singleton service — uses IServiceScopeFactory for scoped EF Core access.
/// Cache key: "sysparam:{tenantId|system}:{key}" (ICacheService handles L1+L2).
/// On SetAsync for feature flags: publishes Redis ff-invalidate channel.
/// </summary>
public class SystemParamService(
    IServiceScopeFactory scopeFactory,
    ICacheService cache,
    IConnectionMultiplexer redis,
    ILogger<SystemParamService> logger) : ISystemParamService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);
    private const string FfInvalidateChannel = "ff-invalidate";

    public Task<T> GetAsync<T>(string key, CancellationToken ct = default)
        => GetInternalAsync<T>(key, null, ct);

    public Task<T> GetAsync<T>(string key, string tenantId, CancellationToken ct = default)
        => GetInternalAsync<T>(key, tenantId, ct);

    public async Task<T> GetOrDefaultAsync<T>(string key, T defaultValue, CancellationToken ct = default)
    {
        try { return await GetAsync<T>(key, ct); }
        catch (KeyNotFoundException) { return defaultValue; }
    }

    public Task SetAsync<T>(string key, T value, CancellationToken ct = default)
        => SetInternalAsync(key, value, null, ct);

    public Task SetAsync<T>(string key, T value, string tenantId, CancellationToken ct = default)
        => SetInternalAsync(key, value, tenantId, ct);

    // --- internals ---

    private async Task<T> GetInternalAsync<T>(string key, string? tenantId, CancellationToken ct)
    {
        var normalizedKey = key.ToLowerInvariant();

        // Try tenant-specific first, then system default
        if (tenantId is not null)
        {
            var tenantCacheKey = CacheKey(normalizedKey, tenantId);
            var tenantCached = await cache.GetAsync<string>(tenantCacheKey, ct);
            if (tenantCached is not null)
                return Deserialize<T>(tenantCached, SystemParamDataType.String); // type resolved below

            var tenantParam = await QueryParamAsync(normalizedKey, tenantId, ct);
            if (tenantParam is not null)
            {
                await cache.SetAsync(tenantCacheKey, tenantParam.Value, CacheTtl, ct);
                return Deserialize<T>(tenantParam.Value, tenantParam.DataType);
            }
        }

        // System-wide lookup
        var sysCacheKey = CacheKey(normalizedKey, null);
        var sysCached = await cache.GetAsync<string>(sysCacheKey, ct);
        if (sysCached is not null)
            return Deserialize<T>(sysCached, SystemParamDataType.String); // raw string cached

        var sysParam = await QueryParamAsync(normalizedKey, null, ct);
        if (sysParam is null)
            throw new KeyNotFoundException($"SystemParameter '{normalizedKey}' not found.");

        await cache.SetAsync(sysCacheKey, sysParam.Value, CacheTtl, ct);
        return Deserialize<T>(sysParam.Value, sysParam.DataType);
    }

    private async Task SetInternalAsync<T>(string key, T value, string? tenantId, CancellationToken ct)
    {
        var normalizedKey = key.ToLowerInvariant();

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SystemParamsDbContext>();

        var param = await db.SystemParameters
            .FirstOrDefaultAsync(p => p.Key == normalizedKey && p.TenantId == tenantId, ct);

        var serialized = Serialize(value);

        if (param is null)
        {
            // Create new param (DataType inferred from T)
            var dataType = InferDataType<T>();
            db.SystemParameters.Add(
                SystemParameter.Create(normalizedKey, serialized, dataType, string.Empty, true, tenantId));
        }
        else
        {
            param.UpdateValue(serialized);
        }

        await db.SaveChangesAsync(ct);

        // Invalidate cache
        await cache.RemoveAsync(CacheKey(normalizedKey, tenantId), ct);

        // If feature flag — publish Redis invalidation for cross-pod propagation (<100ms)
        if (normalizedKey.StartsWith("feature:", StringComparison.OrdinalIgnoreCase))
        {
            var flagKey = tenantId is not null ? $"{normalizedKey}:{tenantId}" : normalizedKey;
            try
            {
                await redis.GetSubscriber().PublishAsync(
                    RedisChannel.Literal(FfInvalidateChannel), flagKey);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Redis ff-invalidate publish failed for {Key} — L0 will expire on 5min reload", flagKey);
            }
        }
    }

    private async Task<SystemParameter?> QueryParamAsync(string key, string? tenantId, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SystemParamsDbContext>();
        return await db.SystemParameters.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Key == key && p.TenantId == tenantId, ct);
    }

    private static string CacheKey(string key, string? tenantId) =>
        $"sysparam:{tenantId ?? "system"}:{key}";

    private static T Deserialize<T>(string value, SystemParamDataType dataType) =>
        dataType switch
        {
            SystemParamDataType.Int    => (T)(object)int.Parse(value),
            SystemParamDataType.Bool   => (T)(object)bool.Parse(value),
            SystemParamDataType.Json   => JsonSerializer.Deserialize<T>(value)
                                          ?? throw new InvalidCastException($"Cannot deserialize '{value}' to {typeof(T).Name}"),
            _                          => typeof(T) == typeof(string)
                                          ? (T)(object)value
                                          : JsonSerializer.Deserialize<T>(value)!
        };

    private static string Serialize<T>(T value) =>
        value switch
        {
            string s => s,
            bool b   => b.ToString().ToLowerInvariant(),
            int i    => i.ToString(),
            _        => JsonSerializer.Serialize(value)
        };

    private static SystemParamDataType InferDataType<T>()
    {
        var t = typeof(T);
        if (t == typeof(string)) return SystemParamDataType.String;
        if (t == typeof(int))    return SystemParamDataType.Int;
        if (t == typeof(bool))   return SystemParamDataType.Bool;
        return SystemParamDataType.Json;
    }
}
