using System.Net;
using System.Text;
using FluentAssertions;
using GSDT.Tests.Integration.Infrastructure;
using Xunit;

namespace GSDT.Tests.Integration.InvestmentProjects;

/// <summary>
/// Verifies CQCQ role is strictly read-only across all 6 investment project types.
/// CQCQ has only *.READ permissions per PermissionSeedDefinitions.RolePermissionMap.
/// PUT/DELETE must return 403 Forbidden.
/// </summary>
[Collection("Integration")]
public class CqcqReadOnlyEnforcementTests(DatabaseFixture db) : IntegrationTestBase(db)
{
    private static readonly string[] Urls =
    [
        "/api/v1/domestic-projects",
        "/api/v1/oda-projects",
        "/api/v1/ppp-projects",
        "/api/v1/dnnn-projects",
        "/api/v1/ndt-projects",
        "/api/v1/fdi-projects",
    ];

    private HttpClient CreateCqcqClient() =>
        CreateAuthenticatedClient(
            roles: ["CQCQ"],
            tenantId: DefaultTenantId.ToString(),
            managingAuthorityId: "00000000-0000-0000-0000-000000000099");

    [Theory]
    [MemberData(nameof(AllUrls))]
    public async Task CqcqRole_Put_Returns403(string url)
    {
        using var client = CreateCqcqClient();
        var fakeId = Guid.NewGuid();
        var body = new StringContent("{}", Encoding.UTF8, "application/json");
        var response = await client.PutAsync($"{url}/{fakeId}", body);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
            $"CQCQ should NOT have WRITE permission for {url}");
    }

    [Theory]
    [MemberData(nameof(AllUrls))]
    public async Task CqcqRole_Delete_Returns403(string url)
    {
        using var client = CreateCqcqClient();
        var fakeId = Guid.NewGuid();
        var response = await client.DeleteAsync($"{url}/{fakeId}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
            $"CQCQ should NOT have DELETE permission for {url}");
    }

    public static IEnumerable<object[]> AllUrls() => Urls.Select(u => new object[] { u });
}
