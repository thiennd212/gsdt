
namespace GSDT.Identity.Application.Authorization;

/// <summary>
/// ABAC handler — checks AttributeRules from DB with 5-minute IMemoryCache.
/// Check order: (1) Admin fast path → Succeed, (2) ABAC rules from cache, (3) DepartmentCode match.
/// </summary>
public sealed class AbacAuthorizationHandler
    : AuthorizationHandler<DepartmentAccessRequirement>
{
    private const int CacheTtlMinutes = 5;

    private readonly IAttributeRuleRepository _rules;
    private readonly IMemoryCache _cache;

    public AbacAuthorizationHandler(IAttributeRuleRepository rules, IMemoryCache cache)
    {
        _rules = rules;
        _cache = cache;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext ctx,
        DepartmentAccessRequirement requirement)
    {
        // (1) RBAC fast path — Admin/SystemAdmin bypasses all ABAC checks
        if (ctx.User.IsInRole(Roles.Admin) || ctx.User.IsInRole(Roles.SystemAdmin))
        {
            ctx.Succeed(requirement);
            return;
        }

        var department = ctx.User.FindFirst("department")?.Value;
        if (string.IsNullOrEmpty(department))
            return; // Deny implicitly — no department claim

        // (2) Load ABAC rules from cache or DB
        var cacheKey = $"abac:rules:department:{department}";
        var rules = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheTtlMinutes);
            return await _rules.GetByAttributeAsync("department", department);
        }) ?? [];

        // Explicit Deny wins over Allow
        if (rules.Any(r => r.Effect == AttributeEffect.Deny))
            return;

        // (3) At least one Allow rule — or user's DepartmentCode matches (fallback)
        if (rules.Any(r => r.Effect == AttributeEffect.Allow))
        {
            ctx.Succeed(requirement);
            return;
        }

        // Fallback: check DepartmentCode claim directly
        var deptCode = ctx.User.FindFirst("department_code")?.Value;
        if (!string.IsNullOrEmpty(deptCode) && deptCode == department)
            ctx.Succeed(requirement);
    }
}
