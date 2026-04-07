using System.Security.Claims;

namespace GSDT.Organization;

/// <summary>
/// Reads org_unit_id and tenant_id JWT claims; resolves ancestor hierarchy via
/// OrgUnitService (cache-backed — safe for sync-over-async after first request).
/// Registered as Scoped — one instance per HTTP request.
/// </summary>
public sealed class JwtOrgContext(IHttpContextAccessor accessor, OrgUnitService orgService)
    : ITenantOrgContext
{
    private IReadOnlyList<Guid>? _cachedHierarchy;

    public Guid? CurrentOrgUnitId =>
        Guid.TryParse(accessor.HttpContext?.User.FindFirstValue("org_unit_id"), out var id)
            ? id : null;

    public bool IsInOrgUnit(Guid orgUnitId) =>
        GetOrgUnitHierarchy().Contains(orgUnitId);

    /// <summary>
    /// Returns ancestor chain [current, parent, grandparent, ...] to root.
    /// Sync-over-async is intentional: OrgUnitService returns from memory cache after first load.
    /// </summary>
    public IReadOnlyList<Guid> GetOrgUnitHierarchy()
    {
        if (_cachedHierarchy is not null) return _cachedHierarchy;

        if (CurrentOrgUnitId is null)
        {
            _cachedHierarchy = [];
            return _cachedHierarchy;
        }

        var tenantIdStr = accessor.HttpContext?.User.FindFirstValue("tenant_id");
        if (!Guid.TryParse(tenantIdStr, out var tenantId))
        {
            _cachedHierarchy = [];
            return _cachedHierarchy;
        }

        // Sync-over-async: safe because tree is in ICacheService (L1 memory) after first load
        _cachedHierarchy = orgService
            .GetAncestorsAsync(CurrentOrgUnitId.Value, tenantId)
            .GetAwaiter()
            .GetResult();

        return _cachedHierarchy;
    }
}
