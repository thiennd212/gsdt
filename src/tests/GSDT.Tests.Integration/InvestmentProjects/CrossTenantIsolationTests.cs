using System.Net;
using FluentAssertions;
using GSDT.Tests.Integration.Infrastructure;
using Xunit;

namespace GSDT.Tests.Integration.InvestmentProjects;

/// <summary>
/// Verifies tenant data isolation at the HTTP level.
/// Two tenants with identical BTC role each get 200 — isolation is at data level.
/// BTC role is used because ProjectQueryScopeService allows system-wide access for BTC;
/// Admin/SystemAdmin roles throw UnauthorizedAccessException in GetScopeFilter.
/// </summary>
[Collection("Integration")]
public class CrossTenantIsolationTests(DatabaseFixture db) : IntegrationTestBase(db)
{
    private static readonly Guid TenantB = Guid.Parse("00000000-0000-0000-0000-000000000002");

    [Fact]
    public async Task DifferentTenants_SameRole_BothGet200()
    {
        // TenantA BTC client and TenantB BTC client both receive 200 —
        // access control passes for both; data isolation is enforced at query level.
        using var clientA = CreateAuthenticatedClient(
            roles: ["BTC"], tenantId: DefaultTenantId.ToString());
        using var clientB = CreateAuthenticatedClient(
            roles: ["BTC"], tenantId: TenantB.ToString());

        var responseA = await clientA.GetAsync("/api/v1/domestic-projects");
        var responseB = await clientB.GetAsync("/api/v1/domestic-projects");

        responseA.StatusCode.Should().Be(HttpStatusCode.OK,
            "TenantA BTC user must have read access to domestic projects");
        responseB.StatusCode.Should().Be(HttpStatusCode.OK,
            "TenantB BTC user must have read access to domestic projects");
    }

    [Fact]
    public async Task TenantB_SeesNoTenantAData()
    {
        // TenantB has no seeded projects — response must be 200 with empty Items array.
        // This proves query-level tenant filtering: TenantB cannot see TenantA data.
        using var clientB = CreateAuthenticatedClient(
            roles: ["BTC"], tenantId: TenantB.ToString());
        var response = await clientB.GetAsync("/api/v1/oda-projects");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var doc = System.Text.Json.JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Envelope must be valid
        root.TryGetProperty("data", out var data).Should().BeTrue("response must have data envelope");
        data.TryGetProperty("items", out var items).Should().BeTrue("data must have items array");
        items.ValueKind.Should().Be(System.Text.Json.JsonValueKind.Array);
        // TenantB has zero seeded projects — items must be empty (no cross-tenant leak)
        items.GetArrayLength().Should().Be(0,
            "TenantB should see 0 ODA projects (no cross-tenant data leak)");
    }
}
