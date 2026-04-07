namespace GSDT.SystemParams.Services;

/// <summary>
/// Full feature flag interface for SystemParams module internal use.
/// Extends the cross-module contract (SharedKernel.Contracts.IFeatureFlagService)
/// with admin methods for reload and invalidation.
/// </summary>
public interface IFeatureFlagService : SharedKernel.Contracts.IFeatureFlagService
{
    /// <summary>Force-reload all flags from DB (called by FeatureFlagRefreshService every 5min).</summary>
    Task ReloadAllAsync(CancellationToken ct = default);

    /// <summary>Invalidate a single flag from L0 cache (called by Redis pub/sub subscriber).</summary>
    void Invalidate(string key);
}
