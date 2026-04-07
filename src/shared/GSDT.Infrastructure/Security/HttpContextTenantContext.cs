using System.Security.Claims;

namespace GSDT.Infrastructure.Security;

/// <summary>
/// Resolves the current tenant from JWT claims in the active HTTP request.
/// Registered as scoped — one instance per request.
/// </summary>
public sealed class HttpContextTenantContext(IHttpContextAccessor httpContextAccessor) : ITenantContext
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? TenantId => Guid.TryParse(
        User?.FindFirstValue("tenant_id"), out var tid) ? tid : null;

    public bool IsSystemAdmin =>
        User?.IsInRole("SystemAdmin") ?? false;
}
