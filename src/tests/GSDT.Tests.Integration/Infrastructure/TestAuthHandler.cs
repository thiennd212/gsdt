using System.Security.Claims;
using System.Text.Encodings.Web;

namespace GSDT.Tests.Integration.Infrastructure;

/// <summary>
/// Bypasses OpenIddict validation — injects ClaimsPrincipal directly from test headers.
/// Only active when ASPNETCORE_ENVIRONMENT=Testing.
/// </summary>
public class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "TestScheme";
    public const string UserIdHeader = "X-Test-UserId";
    public const string RolesHeader = "X-Test-Roles";
    public const string TenantIdHeader = "X-Test-TenantId";
    public const string ManagingAuthorityIdHeader = "X-Test-ManagingAuthorityId";
    public const string ProjectOwnerIdHeader = "X-Test-ProjectOwnerId";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Only authenticate if the test identity header is present.
        // Requests without X-Test-UserId are treated as unauthenticated → 401 via challenge.
        if (!Request.Headers.ContainsKey(UserIdHeader))
            return Task.FromResult(AuthenticateResult.NoResult());

        var userId = Request.Headers[UserIdHeader].FirstOrDefault()!;
        var roles = Request.Headers[RolesHeader].FirstOrDefault()?.Split(',') ?? [];
        var tenantId = Request.Headers[TenantIdHeader].FirstOrDefault();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, "Test User"),
            new(ClaimTypes.Email, "test@test.vn"),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        if (tenantId != null) claims.Add(new("tenant_id", tenantId));

        var managingAuthorityId = Request.Headers[ManagingAuthorityIdHeader].FirstOrDefault();
        if (managingAuthorityId != null) claims.Add(new("managing_authority_id", managingAuthorityId));

        var projectOwnerId = Request.Headers[ProjectOwnerIdHeader].FirstOrDefault();
        if (projectOwnerId != null) claims.Add(new("project_owner_id", projectOwnerId));

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        return Task.FromResult(AuthenticateResult.Success(
            new AuthenticationTicket(principal, SchemeName)));
    }
}
