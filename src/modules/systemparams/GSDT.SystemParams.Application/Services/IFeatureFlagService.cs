namespace GSDT.SystemParams.Application.Services;

/// <summary>
/// Sync feature flag reads backed by ConcurrentDict L0 cache (O(1)).
/// Per-tenant key stored as "{flagKey}:{tenantId}" with fallback to "{flagKey}".
/// </summary>
public interface IFeatureFlagService
{
    bool IsEnabled(string key, string? tenantId = null);
    void Invalidate(string key);
    Task ReloadAllAsync(CancellationToken ct = default);
}
