namespace GSDT.Api.Controllers;

/// <summary>
/// E2E test-only endpoint: returns a fake OIDC user object for Playwright sessionStorage injection.
/// Only registered when ASPNETCORE_ENVIRONMENT is Development or Testing.
/// DO NOT deploy to production.
/// </summary>
[ApiController]
[Route("api/v1/test")]
public class TestTokenController : ControllerBase
{
    private static readonly Guid TestTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    /// <summary>
    /// GET /api/v1/test/token?role=BTC|CQCQ|CDT
    /// Returns an OIDC-compatible user object that can be injected into sessionStorage
    /// to authenticate Playwright E2E tests without a real OIDC flow.
    /// </summary>
    [HttpGet("token")]
    [AllowAnonymous]
    public IActionResult GetTestToken([FromQuery] string role = "BTC")
    {
        if (!HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment()
            && !HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().EnvironmentName.Equals("Testing", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound();
        }

        var userId = role.ToUpperInvariant() switch
        {
            "BTC" => Guid.Parse("10000000-0000-0000-0000-000000000001"),
            "CQCQ" => Guid.Parse("20000000-0000-0000-0000-000000000002"),
            "CDT" => Guid.Parse("30000000-0000-0000-0000-000000000003"),
            _ => Guid.Parse("10000000-0000-0000-0000-000000000001"),
        };

        var roles = role.ToUpperInvariant() switch
        {
            "BTC" => new[] { "Admin", "SystemAdmin", "BTC" },
            "CQCQ" => new[] { "CQCQ" },
            "CDT" => new[] { "CDT" },
            _ => new[] { "Admin", "BTC" },
        };

        // OIDC user object format compatible with oidc-client-ts WebStorageStateStore
        var oidcUser = new
        {
            id_token = "e2e-test-token",
            access_token = $"e2e-{role.ToLowerInvariant()}-{Guid.NewGuid():N}",
            token_type = "Bearer",
            scope = "openid profile roles",
            expires_at = DateTimeOffset.UtcNow.AddHours(8).ToUnixTimeSeconds(),
            profile = new
            {
                sub = userId.ToString(),
                name = $"E2E Test User ({role})",
                preferred_username = $"e2e-{role.ToLowerInvariant()}@test.gsdt.vn",
                email = $"e2e-{role.ToLowerInvariant()}@test.gsdt.vn",
                role = roles,
                tenant_id = TestTenantId.ToString(),
            },
        };

        return Ok(new { data = oidcUser });
    }

    /// <summary>
    /// DELETE /api/v1/test/cleanup?prefix=e2e-xxx
    /// Cleans up test data created during E2E runs (by name prefix).
    /// </summary>
    [HttpDelete("cleanup")]
    [AllowAnonymous]
    public IActionResult Cleanup([FromQuery] string? prefix)
    {
        if (!HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment()
            && !HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().EnvironmentName.Equals("Testing", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound();
        }

        // Placeholder — actual cleanup implementation added when E2E tests create data
        return Ok(new { message = $"Cleanup requested for prefix: {prefix}" });
    }
}
