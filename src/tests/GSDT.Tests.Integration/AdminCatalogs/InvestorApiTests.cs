using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace GSDT.Tests.Integration.AdminCatalogs;

/// <summary>
/// Integration tests for InvestorsController — /api/v1/masterdata/investors
/// Chủ đầu tư: flat tenant-scoped catalog, filter by investorType.
/// Read: [Authorize(Roles = "BTC,CQCQ,CDT")]
/// Write (POST/PUT/DELETE): [Authorize(Roles = "BTC")]
/// Default Client is pre-authenticated as SystemAdmin — passes all role checks.
/// </summary>
[Collection("Integration")]
public class InvestorApiTests(DatabaseFixture db) : IntegrationTestBase(db)
{
    private const string BaseUrl = "/api/v1/masterdata/investors";

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Minimal valid create body. BusinessIdOrCccd must be unique per tenant.
    /// Uses a short GUID suffix to guarantee uniqueness across parallel test runs.
    /// </summary>
    private static object ValidCreateBody(string? businessId = null) => new
    {
        investorType      = "Enterprise",
        businessIdOrCccd  = businessId ?? $"BIZ-{Guid.NewGuid():N}"[..15],
        nameVi            = "Công ty Test",
        nameEn            = (string?)"Test Company",
    };

    /// <summary>Creates an investor and returns its id.</summary>
    private async Task<Guid> CreateInvestorAsync(string? businessId = null)
    {
        var response = await Client.PostAsJsonAsync(BaseUrl, ValidCreateBody(businessId));
        response.StatusCode.Should().Be(HttpStatusCode.OK, "pre-condition: investor creation must succeed");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("data").GetProperty("id").GetGuid();
    }

    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task List_Returns200()
    {
        var response = await Client.GetAsync(BaseUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.TryGetProperty("data", out _).Should().BeTrue("response must be wrapped in ApiResponse envelope");
    }

    [Fact]
    public async Task Create_Returns200()
    {
        var response = await Client.PostAsJsonAsync(BaseUrl, ValidCreateBody());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("data").TryGetProperty("id", out var idProp).Should().BeTrue();
        idProp.GetGuid().Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetById_Returns200()
    {
        var id = await CreateInvestorAsync();

        var response = await Client.GetAsync($"{BaseUrl}/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("data").TryGetProperty("id", out var idProp).Should().BeTrue();
        idProp.GetGuid().Should().Be(id);
    }

    [Fact]
    public async Task Delete_Returns204()
    {
        var id = await CreateInvestorAsync();

        var response = await Client.DeleteAsync($"{BaseUrl}/{id}");

        // Soft delete returns 204 NoContent per controller implementation
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
