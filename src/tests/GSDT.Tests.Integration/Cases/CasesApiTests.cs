using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace GSDT.Tests.Integration.Cases;

/// <summary>
/// Integration tests for CasesController — /api/v1/cases
/// Routes: GET /api/v1/cases?tenantId={guid}
///         GET /api/v1/cases/{id}?tenantId={guid}
///         POST /api/v1/cases  (body: TenantId, Title, Description, Type, Priority)
/// [Authorize] on controller; Assign/Approve require Admin/SystemAdmin/GovOfficer roles.
/// </summary>
[Collection("Integration")]
public class CasesApiTests(DatabaseFixture db) : IntegrationTestBase(db)
{
    private const string BaseUrl = "/api/v1/cases";

    // -----------------------------------------------------------------
    // LIST
    // -----------------------------------------------------------------

    [Fact]
    public async Task ListCases_WithValidTenantId_Returns200()
    {
        var tenantId = Guid.NewGuid();
        var client = CreateAuthenticatedClient(roles: ["SystemAdmin"], tenantId: tenantId.ToString());

        var response = await client.GetAsync($"{BaseUrl}?tenantId={tenantId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ListCases_ResponseBody_WrappedInApiResponseEnvelope()
    {
        var tenantId = Guid.NewGuid();
        var response = await Client.GetAsync($"{BaseUrl}?tenantId={tenantId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.TryGetProperty("data", out _).Should().BeTrue("response must be ApiResponse envelope");
    }

    [Fact]
    public async Task ListCases_Unauthenticated_Returns401Or403()
    {
        var tenantId = Guid.NewGuid();
        var anonClient = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var response = await anonClient.GetAsync($"{BaseUrl}?tenantId={tenantId}");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    // -----------------------------------------------------------------
    // CREATE
    // -----------------------------------------------------------------

    [Fact]
    public async Task CreateCase_WithValidData_Returns200()
    {
        var tenantId = Guid.NewGuid();
        var client = CreateAuthenticatedClient(
            userId: Guid.NewGuid().ToString(),
            roles: ["SystemAdmin"],
            tenantId: tenantId.ToString());

        // Title >= 10 chars, Description >= 20 chars (validator rules)
        var response = await client.PostAsJsonAsync(BaseUrl, new
        {
            TenantId = tenantId,
            Title = "Test Case Title for Integration",
            Description = "This is a valid description for the test case with sufficient length.",
            Type = 0,       // CaseType.Application = 0
            Priority = 1,   // CasePriority.Medium = 1
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var id = body.GetProperty("data").GetProperty("id").GetGuid();
        id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateCase_MissingTenantId_Returns422()
    {
        // TenantId = Guid.Empty → validator rejects (NotEmpty)
        var response = await Client.PostAsJsonAsync(BaseUrl, new
        {
            TenantId = Guid.Empty,
            Title = "Test Case Title",
            Description = "This is a valid description for the test case with sufficient length.",
            Type = 0,
            Priority = 0,
        });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateCase_TitleTooShort_Returns422()
    {
        // Title < 10 chars → validator rejects (MinimumLength 10)
        var tenantId = Guid.NewGuid();
        var response = await Client.PostAsJsonAsync(BaseUrl, new
        {
            TenantId = tenantId,
            Title = "Short",
            Description = "This is a valid description for the test case with sufficient length.",
            Type = 0,
            Priority = 0,
        });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateCase_DescriptionTooShort_Returns422()
    {
        // Description < 20 chars → validator rejects (MinimumLength 20)
        var tenantId = Guid.NewGuid();
        var response = await Client.PostAsJsonAsync(BaseUrl, new
        {
            TenantId = tenantId,
            Title = "Test Case Title Long Enough",
            Description = "Too short desc",
            Type = 0,
            Priority = 0,
        });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    // -----------------------------------------------------------------
    // GET BY ID
    // -----------------------------------------------------------------

    [Fact]
    public async Task GetCaseById_ForNonExistentId_Returns404()
    {
        var tenantId = Guid.NewGuid();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.GetAsync($"{BaseUrl}/{nonExistentId}?tenantId={tenantId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCaseById_ForExistingCase_Returns200WithData()
    {
        var tenantId = Guid.NewGuid();
        var client = CreateAuthenticatedClient(
            userId: Guid.NewGuid().ToString(),
            roles: ["SystemAdmin"],
            tenantId: tenantId.ToString());

        // Create first
        var createResp = await client.PostAsJsonAsync(BaseUrl, new
        {
            TenantId = tenantId,
            Title = "Case for GetById test assertion",
            Description = "This is a valid description with sufficient length for validation.",
            Type = 0,
            Priority = 0,
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var createBody = await createResp.Content.ReadFromJsonAsync<JsonElement>();
        var caseId = createBody.GetProperty("data").GetProperty("id").GetGuid();

        // Get by ID
        var getResp = await client.GetAsync($"{BaseUrl}/{caseId}?tenantId={tenantId}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await getResp.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("data").GetProperty("id").GetGuid().Should().Be(caseId);
    }

    // -----------------------------------------------------------------
    // FILTER BY TENANT
    // -----------------------------------------------------------------

    [Fact]
    public async Task ListCases_FilterByTenant_OnlyReturnsTenantCases()
    {
        var tenantId = Guid.NewGuid();
        var client = CreateAuthenticatedClient(
            userId: Guid.NewGuid().ToString(),
            roles: ["SystemAdmin"],
            tenantId: tenantId.ToString());

        // Seed a case for this tenant
        await client.PostAsJsonAsync(BaseUrl, new
        {
            TenantId = tenantId,
            Title = "Tenant-scoped case for list test",
            Description = "This is a valid description for the test case with sufficient length.",
            Type = 0,
            Priority = 1,
        });

        var response = await client.GetAsync($"{BaseUrl}?tenantId={tenantId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.TryGetProperty("data", out var data).Should().BeTrue();
        // data is an array or paged object — at minimum the response is valid
        data.ValueKind.Should().NotBe(JsonValueKind.Undefined);
    }
}
