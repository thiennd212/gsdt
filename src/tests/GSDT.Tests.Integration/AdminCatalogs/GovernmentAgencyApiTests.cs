using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace GSDT.Tests.Integration.AdminCatalogs;

/// <summary>
/// Integration tests for GovernmentAgenciesController — /api/v1/masterdata/government-agencies
/// Cơ quan chủ quản: hierarchical tenant-scoped catalog (max 4 levels).
/// Read: [Authorize(Roles = "BTC,CQCQ,CDT")]
/// Write (POST/PUT/DELETE): [Authorize(Roles = "BTC")]
/// Tests use role-specific clients — SystemAdmin is NOT in BTC/CQCQ/CDT.
/// </summary>
[Collection("Integration")]
public class GovernmentAgencyApiTests(DatabaseFixture db) : IntegrationTestBase(db)
{
    private const string BaseUrl = "/api/v1/masterdata/government-agencies";

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Creates an HttpClient with BTC role (satisfies both read and write auth).</summary>
    private HttpClient CreateBtcClient() =>
        CreateAuthenticatedClient(roles: ["BTC"], tenantId: DefaultTenantId.ToString());

    /// <summary>Minimal valid create body with a guaranteed-unique code.</summary>
    private static object ValidCreateBody(string? code = null, Guid? parentId = null) => new
    {
        name                = "Test Government Agency",
        code                = code ?? $"GA-{Guid.NewGuid():N}"[..12],
        parentId,
        agencyType          = "Ministry",
        origin              = (string?)null,
        ldaServer           = (string?)null,
        address             = (string?)null,
        phone               = (string?)null,
        fax                 = (string?)null,
        email               = (string?)null,
        notes               = (string?)null,
        sortOrder           = 0,
        reportDisplayOrder  = (int?)null,
    };

    /// <summary>Creates a government agency via BTC client and returns its id.</summary>
    private async Task<Guid> CreateAgencyAsync(string? code = null, Guid? parentId = null)
    {
        using var client = CreateBtcClient();
        var response = await client.PostAsJsonAsync(BaseUrl, ValidCreateBody(code, parentId));
        response.StatusCode.Should().Be(HttpStatusCode.OK, "pre-condition: agency creation must succeed");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("data").GetProperty("id").GetGuid();
    }

    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task List_Returns200()
    {
        using var client = CreateBtcClient();
        var response = await client.GetAsync(BaseUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.TryGetProperty("data", out _).Should().BeTrue("response must be wrapped in ApiResponse envelope");
    }

    [Fact]
    public async Task Create_Returns200()
    {
        using var client = CreateBtcClient();
        var response = await client.PostAsJsonAsync(BaseUrl, ValidCreateBody());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("data").TryGetProperty("id", out var idProp).Should().BeTrue();
        idProp.GetGuid().Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetById_Returns200()
    {
        var id = await CreateAgencyAsync();

        using var client = CreateBtcClient();
        var response = await client.GetAsync($"{BaseUrl}/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("data").TryGetProperty("id", out var idProp).Should().BeTrue();
        idProp.GetGuid().Should().Be(id);
    }

    [Fact]
    public async Task Update_Returns204()
    {
        var id = await CreateAgencyAsync();

        var updateBody = new
        {
            name               = "Updated Agency Name",
            code               = $"GA-UPD-{Guid.NewGuid():N}"[..12],
            parentId           = (Guid?)null,
            agencyType         = "Department",
            origin             = (string?)null,
            ldaServer          = (string?)null,
            address            = "123 Main St",
            phone              = (string?)null,
            fax                = (string?)null,
            email              = (string?)null,
            notes              = (string?)null,
            sortOrder          = 1,
            reportDisplayOrder = (int?)null,
            isActive           = true,
        };

        using var client = CreateBtcClient();
        var response = await client.PutAsJsonAsync($"{BaseUrl}/{id}", updateBody);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_Returns204()
    {
        var id = await CreateAgencyAsync();

        using var client = CreateBtcClient();
        var response = await client.DeleteAsync($"{BaseUrl}/{id}");

        // Soft delete returns 204 NoContent per controller implementation
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Create_WithParent_HierarchyCorrect()
    {
        // Create a root-level agency first
        var parentId = await CreateAgencyAsync();

        // Create a child agency referencing the parent
        var childId = await CreateAgencyAsync(parentId: parentId);

        // Fetch child and verify parentId is set correctly
        using var client = CreateBtcClient();
        var response = await client.GetAsync($"{BaseUrl}/{childId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var data = body.GetProperty("data");
        data.TryGetProperty("parentId", out var parentProp).Should().BeTrue();
        parentProp.GetGuid().Should().Be(parentId);
    }
}
