namespace GSDT.Identity.Application.Authorization;

/// <summary>
/// Invalidates cached ABAC attribute rules when rules are created, updated, or deleted.
/// Call after any write operation on AttributeRule to prevent stale authorization decisions (F-17).
/// Cache key scope: department attribute value (matches AbacAuthorizationHandler key pattern).
/// </summary>
public interface IAbacCacheInvalidator
{
    /// <summary>Remove cached rules for the given attribute value (e.g. department code).</summary>
    void InvalidateByAttributeValue(string attributeValue);

    /// <summary>Remove all cached ABAC rule entries — use after bulk rule changes.</summary>
    void InvalidateAll();
}
