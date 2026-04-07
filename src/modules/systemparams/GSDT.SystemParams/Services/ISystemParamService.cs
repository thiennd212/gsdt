namespace GSDT.SystemParams.Services;

/// <summary>
/// Runtime typed config access — injected by any module.
/// Cache-aside (TTL 5min). Tenant override falls back to system-wide default.
/// Register as singleton (uses IServiceScopeFactory for scoped DB access).
/// </summary>
public interface ISystemParamService
{
    /// <summary>Get system-wide param, deserialised to T.</summary>
    Task<T> GetAsync<T>(string key, CancellationToken ct = default);

    /// <summary>Get tenant-specific param, falls back to system-wide default.</summary>
    Task<T> GetAsync<T>(string key, string tenantId, CancellationToken ct = default);

    /// <summary>Get with explicit default — no exception if key not found.</summary>
    Task<T> GetOrDefaultAsync<T>(string key, T defaultValue, CancellationToken ct = default);

    /// <summary>Set system-wide param — persists + invalidates cache.</summary>
    Task SetAsync<T>(string key, T value, CancellationToken ct = default);

    /// <summary>Set tenant-specific param override — persists + invalidates cache.</summary>
    Task SetAsync<T>(string key, T value, string tenantId, CancellationToken ct = default);
}
