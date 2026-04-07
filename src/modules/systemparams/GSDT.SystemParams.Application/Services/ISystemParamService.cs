namespace GSDT.SystemParams.Application.Services;

/// <summary>
/// Async key-value store backed by DB + Redis.
/// SetAsync persists to DB and invalidates the Redis cache key.
/// </summary>
public interface ISystemParamService
{
    Task<string?> GetAsync(string key, string? tenantId = null, CancellationToken ct = default);
    Task<T?> GetAsync<T>(string key, string? tenantId = null, CancellationToken ct = default);
    Task SetAsync(string key, object value, CancellationToken ct = default);
    Task SetAsync(string key, object value, string tenantId, CancellationToken ct = default);
}
