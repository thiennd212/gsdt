using System.Security.Claims;

namespace GSDT.Tests.Integration.Infrastructure;

/// <summary>
/// ICurrentUser implementation for tests — reads from ClaimsPrincipal injected by TestAuthHandler.
/// </summary>
public class TestCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid UserId => Guid.TryParse(
        User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : Guid.Empty;

    public Guid? TenantId => Guid.TryParse(
        User?.FindFirstValue("tenant_id"), out var tid) ? tid : null;

    public string UserName =>
        User?.FindFirstValue(ClaimTypes.Name) ?? "TestUser";

    public string[] Roles => User?.Claims
        .Where(c => c.Type == ClaimTypes.Role)
        .Select(c => c.Value).ToArray() ?? [];

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public Guid? ManagingAuthorityId => Guid.TryParse(
        User?.FindFirstValue("managing_authority_id"), out var mid) ? mid : null;

    public Guid? ProjectOwnerId => Guid.TryParse(
        User?.FindFirstValue("project_owner_id"), out var pid) ? pid : null;
}
