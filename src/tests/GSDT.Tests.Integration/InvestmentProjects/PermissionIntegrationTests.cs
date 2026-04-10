using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GSDT.Tests.Integration.InvestmentProjects;

/// <summary>
/// HTTP-level tests for [RequirePermission] authorization on all 6 investment controllers.
/// Uses real seeded users from TestPermissionSeeder — EffectivePermissionService queries DB.
/// Verifies that:
///   - Unauthenticated requests get 401/403
///   - BTC role can READ all project types
///   - CQCQ role can READ all project types
///   - CQCQ role CANNOT WRITE (gets 403)
///   - BTC role CAN WRITE (gets 400 validation, not 403)
///   - BTC role CAN DELETE (gets 404 not found, not 403)
/// </summary>
[Collection("Integration")]
public class PermissionIntegrationTests(DatabaseFixture db) : IntegrationTestBase(db)
{
    // Endpoints for all 6 project types — name + list URL
    private static readonly (string Name, string ListUrl)[] Endpoints =
    [
        ("Domestic", "/api/v1/domestic-projects"),
        ("ODA",      "/api/v1/oda-projects"),
        ("PPP",      "/api/v1/ppp-projects"),
        ("DNNN",     "/api/v1/dnnn-projects"),
        ("NDT",      "/api/v1/ndt-projects"),
        ("FDI",      "/api/v1/fdi-projects"),
    ];

    // Fixed managing authority ID injected for CQCQ role — scope filter requires this claim.
    // Value is a deterministic test GUID; the query may return 0 rows but will not throw 401.
    private static readonly Guid TestManagingAuthorityId = Guid.Parse("00000000-0000-0000-0000-000000000002");

    /// <summary>
    /// Creates HttpClient using a real seeded test user for the specified role.
    /// Auto-resolves to a real userId via ApiFactory.CreateAuthenticatedClient.
    /// CQCQ role additionally receives a managing_authority_id claim so ProjectQueryScopeService
    /// can apply the scope filter without throwing UnauthorizedAccessException.
    /// </summary>
    private HttpClient CreateRoleClient(string role)
    {
        var managingAuthorityId = role == "CQCQ" ? TestManagingAuthorityId.ToString() : null;
        return CreateAuthenticatedClient(
            roles: [role],
            tenantId: DefaultTenantId.ToString(),
            managingAuthorityId: managingAuthorityId);
    }

    // ── 1. Unauthenticated → 401 (6 tests via Theory) ────────────────────────

    [Theory]
    [MemberData(nameof(AllEndpoints))]
    public async Task Unauthenticated_Returns401(string name, string url)
    {
        using var anonClient = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
        var response = await anonClient.GetAsync(url);
        response.StatusCode.Should().BeOneOf(
            [HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden],
            $"{name} endpoint should reject unauthenticated requests");
    }

    // ── 2. BTC role → can READ all types (6 tests via Theory) ────────────────

    [Theory]
    [MemberData(nameof(AllEndpoints))]
    public async Task BtcRole_CanRead_AllProjectTypes(string name, string url)
    {
        using var client = CreateRoleClient("BTC");
        var response = await client.GetAsync(url);
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            $"BTC should have READ permission for {name}");
    }

    // ── 3. CQCQ role → can READ all types (6 tests via Theory) ──────────────

    [Theory]
    [MemberData(nameof(AllEndpoints))]
    public async Task CqcqRole_CanRead_AllProjectTypes(string name, string url)
    {
        using var client = CreateRoleClient("CQCQ");
        var response = await client.GetAsync(url);
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            $"CQCQ should have READ permission for {name}");
    }

    // ── 4. CQCQ role → cannot WRITE (POST returns 403) (6 tests via Theory) ─

    [Theory]
    [MemberData(nameof(AllEndpoints))]
    public async Task CqcqRole_CannotWrite_Returns403(string name, string url)
    {
        using var client = CreateRoleClient("CQCQ");
        var body = new StringContent("{}", Encoding.UTF8, "application/json");
        var response = await client.PostAsync(url, body);
        // 403 = permission denied (no WRITE permission). Not 401 (auth passed).
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
            $"CQCQ should NOT have WRITE permission for {name}");
    }

    // ── 5. BTC can WRITE (POST accepted or 400 validation, NOT 403) ──────────

    [Fact]
    public async Task BtcRole_CanWrite_DomesticProjects()
    {
        using var client = CreateRoleClient("BTC");
        var body = new StringContent("{}", Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/v1/domestic-projects", body);
        // 400 = validation error (auth PASSED, body invalid → proves WRITE permission works)
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden,
            "BTC should have WRITE permission for Domestic projects");
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task BtcRole_CanDelete_DomesticProjects()
    {
        using var client = CreateRoleClient("BTC");
        var fakeId = Guid.NewGuid();
        var response = await client.DeleteAsync($"/api/v1/domestic-projects/{fakeId}");
        // 404 = not found (auth PASSED). Not 403 = proves DELETE permission works.
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden,
            "BTC should have DELETE permission for Domestic projects");
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    // ── Test data provider ───────────────────────────────────────────────────

    public static IEnumerable<object[]> AllEndpoints()
        => Endpoints.Select(e => new object[] { e.Name, e.ListUrl });
}
