
namespace GSDT.Identity.Infrastructure.Authorization;

/// <summary>
/// Removes IMemoryCache entries written by AbacAuthorizationHandler.
/// Cache key pattern: "abac:rules:department:{attributeValue}" — must match handler exactly.
/// Inject wherever AttributeRule write operations occur (F-17).
/// </summary>
public sealed class AbacCacheInvalidator : IAbacCacheInvalidator
{
    // Prefix must match AbacAuthorizationHandler.cacheKey format
    private const string KeyPrefix = "abac:rules:department:";

    private readonly IMemoryCache _cache;

    public AbacCacheInvalidator(IMemoryCache cache) => _cache = cache;

    /// <inheritdoc/>
    public void InvalidateByAttributeValue(string attributeValue)
        => _cache.Remove($"{KeyPrefix}{attributeValue}");

    /// <inheritdoc/>
    public void InvalidateAll()
    {
        // IMemoryCache has no bulk-remove API; compact to evict all expired/excess entries.
        // For targeted invalidation, callers should prefer InvalidateByAttributeValue.
        if (_cache is MemoryCache mc)
            mc.Compact(1.0);
    }
}
