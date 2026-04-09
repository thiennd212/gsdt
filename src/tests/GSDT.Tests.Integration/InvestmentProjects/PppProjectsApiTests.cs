using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace GSDT.Tests.Integration.InvestmentProjects;

/// <summary>
/// Integration tests for PppProjectsController — /api/v1/ppp-projects.
/// Client is pre-authenticated as SystemAdmin with tenantId 00000000-0000-0000-0000-000000000001.
/// SystemAdmin satisfies all role checks (BTC, CQCQ, CDT) because the test auth handler
/// passes roles directly from the header.
/// For role-restricted tests, we create a role-specific client.
/// </summary>
[Collection("Integration")]
public class PppProjectsApiTests(DatabaseFixture db) : IntegrationTestBase(db)
{
    private const string BaseUrl = "/api/v1/ppp-projects";

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates an authenticated client with BTC + CDT roles (write access).
    /// SystemAdmin role is not in the controller's [Authorize(Roles = "BTC,CDT")] list,
    /// so write tests use a client that carries BTC role explicitly.
    /// </summary>
    private HttpClient CreateWriteClient() =>
        CreateAuthenticatedClient(roles: ["BTC", "CDT"], tenantId: DefaultTenantId.ToString());

    /// <summary>Returns a minimal valid PPP create body with a unique projectCode.</summary>
    private static object BuildCreateBody(string suffix) => new
    {
        projectCode = $"PPP-{suffix}",
        projectName = $"PPP Test Project {suffix}",
        managingAuthorityId = Guid.NewGuid(),
        industrySectorId = Guid.NewGuid(),
        projectOwnerId = Guid.NewGuid(),
        projectGroupId = Guid.NewGuid(),
        contractType = 1,            // BOT
        subProjectType = 0,
        statusId = Guid.NewGuid(),
        prelimTotalInvestment = 1000m,
        prelimStateCapital = 400m,
        prelimEquityCapital = 300m,
        prelimLoanCapital = 300m,
        // All optional fields omitted — nulls are fine
        competentAuthorityId = (Guid?)null,
        preparationUnit = (string?)null,
        objective = (string?)null,
        areaHectares = (decimal?)null,
        capacity = (string?)null,
        mainItems = (string?)null,
        projectManagementUnitId = (Guid?)null,
        pmuDirectorName = (string?)null,
        pmuPhone = (string?)null,
        pmuEmail = (string?)null,
        implementationPeriod = (string?)null,
        policyDecisionNumber = (string?)null,
        policyDecisionDate = (DateTime?)null,
        policyDecisionAuthority = (string?)null,
        policyDecisionPerson = (string?)null,
        policyDecisionFileId = (Guid?)null,
    };

    // ── Core CRUD ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task List_AsAuthenticated_Returns200()
    {
        // GET list requires BTC, CQCQ, or CDT
        using var client = CreateAuthenticatedClient(
            roles: ["BTC"], tenantId: DefaultTenantId.ToString());

        var response = await client.GetAsync(BaseUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Create_ValidProject_Returns200WithId()
    {
        using var client = CreateWriteClient();
        var body = BuildCreateBody(Guid.NewGuid().ToString("N")[..8]);

        var response = await client.PostAsJsonAsync(BaseUrl, body);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.TryGetProperty("data", out var data).Should().BeTrue("ApiResponse must have 'data'");
        // data is the new project Guid
        data.ValueKind.Should().Be(JsonValueKind.String);
        Guid.TryParse(data.GetString(), out _).Should().BeTrue("data should be a valid Guid");
    }

    [Fact]
    public async Task GetById_AfterCreate_Returns200WithCorrectData()
    {
        using var client = CreateWriteClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var expectedCode = $"PPP-{suffix}";
        var body = BuildCreateBody(suffix);

        // Create
        var createResp = await client.PostAsJsonAsync(BaseUrl, body);
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var createJson = await createResp.Content.ReadFromJsonAsync<JsonElement>();
        var newId = createJson.GetProperty("data").GetString()!;

        // Read back
        using var readClient = CreateAuthenticatedClient(
            roles: ["BTC"], tenantId: DefaultTenantId.ToString());
        var getResp = await readClient.GetAsync($"{BaseUrl}/{newId}");

        getResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var getJson = await getResp.Content.ReadFromJsonAsync<JsonElement>();
        var project = getJson.GetProperty("data");
        project.GetProperty("projectCode").GetString().Should().Be(expectedCode);
    }

    [Fact]
    public async Task Update_ExistingProject_Returns200()
    {
        using var client = CreateWriteClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];

        // Step 1: create
        var body = BuildCreateBody(suffix);
        var createResp = await client.PostAsJsonAsync(BaseUrl, body);
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var newId = (await createResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("data").GetString()!;

        // Step 2: get to obtain rowVersion
        using var readClient = CreateAuthenticatedClient(
            roles: ["BTC"], tenantId: DefaultTenantId.ToString());
        var getResp = await readClient.GetAsync($"{BaseUrl}/{newId}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var getJson = await getResp.Content.ReadFromJsonAsync<JsonElement>();
        // rowVersion is base64-encoded bytes in JSON
        var rowVersionBase64 = getJson.GetProperty("data").GetProperty("rowVersion").GetString()!;
        var rowVersionBytes = Convert.FromBase64String(rowVersionBase64);

        // Step 3: update — send updated projectName
        var updateBody = new
        {
            id = Guid.Parse(newId),
            rowVersion = rowVersionBytes,
            projectCode = $"PPP-{suffix}",
            projectName = $"PPP Updated {suffix}",
            managingAuthorityId = Guid.NewGuid(),
            industrySectorId = Guid.NewGuid(),
            projectOwnerId = Guid.NewGuid(),
            projectGroupId = Guid.NewGuid(),
            contractType = 1,
            subProjectType = 0,
            statusId = Guid.NewGuid(),
            prelimTotalInvestment = 2000m,
            prelimStateCapital = 800m,
            prelimEquityCapital = 600m,
            prelimLoanCapital = 600m,
            competentAuthorityId = (Guid?)null,
            preparationUnit = (string?)null,
            objective = (string?)null,
            areaHectares = (decimal?)null,
            capacity = (string?)null,
            mainItems = (string?)null,
            stopContent = (string?)null,
            stopDecisionNumber = (string?)null,
            stopDecisionDate = (DateTime?)null,
            stopFileId = (Guid?)null,
            projectManagementUnitId = (Guid?)null,
            pmuDirectorName = (string?)null,
            pmuPhone = (string?)null,
            pmuEmail = (string?)null,
            implementationPeriod = (string?)null,
            policyDecisionNumber = (string?)null,
            policyDecisionDate = (DateTime?)null,
            policyDecisionAuthority = (string?)null,
            policyDecisionPerson = (string?)null,
            policyDecisionFileId = (Guid?)null,
        };

        var putResp = await client.PutAsJsonAsync($"{BaseUrl}/{newId}", updateBody);

        putResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_ExistingProject_Returns200()
    {
        using var client = CreateWriteClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];

        // Create first
        var createResp = await client.PostAsJsonAsync(BaseUrl, BuildCreateBody(suffix));
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var newId = (await createResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("data").GetString()!;

        // Delete
        var deleteResp = await client.DeleteAsync($"{BaseUrl}/{newId}");

        deleteResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── Validation ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_MissingRequiredFields_Returns400()
    {
        using var client = CreateWriteClient();
        // Empty object — missing projectCode, projectName, Guid refs
        var emptyBody = new { };

        var response = await client.PostAsJsonAsync(BaseUrl, emptyBody);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_Unauthenticated_Returns401Or403()
    {
        // Anonymous client — no X-Test-UserId header → TestAuthHandler returns NoResult
        var anonClient = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var response = await anonClient.PostAsJsonAsync(BaseUrl, BuildCreateBody("anon"));

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    // ── Sub-entities: Decisions ───────────────────────────────────────────────

    [Fact]
    public async Task AddDecision_ToProject_Returns200()
    {
        using var client = CreateWriteClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var createResp = await client.PostAsJsonAsync(BaseUrl, BuildCreateBody(suffix));
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var projectId = (await createResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("data").GetString()!;

        // Decision capital: Total == State + Equity + Loan; State == Central + Local + Other
        var decisionBody = new
        {
            decisionType = 1,
            decisionNumber = $"QD-PPP-{suffix}",
            decisionDate = DateTime.UtcNow.Date,
            decisionAuthority = "Bo Ke Hoach va Dau Tu",
            decisionPerson = (string?)null,
            totalInvestment = 1000m,
            stateCapital = 400m,
            centralBudget = 200m,
            localBudget = 100m,
            otherStateBudget = 100m,
            equityCapital = 300m,
            loanCapital = 300m,
            equityRatio = (decimal?)null,
            adjustmentContentId = (Guid?)null,
            notes = (string?)null,
            fileId = (Guid?)null,
        };

        var response = await client.PostAsJsonAsync($"{BaseUrl}/{projectId}/decisions", decisionBody);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteDecision_Returns200()
    {
        using var client = CreateWriteClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];

        // Create project
        var createResp = await client.PostAsJsonAsync(BaseUrl, BuildCreateBody(suffix));
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var projectId = (await createResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("data").GetString()!;

        // Add decision
        var decisionBody = new
        {
            decisionType = 1,
            decisionNumber = $"QD-PPP-DEL-{suffix}",
            decisionDate = DateTime.UtcNow.Date,
            decisionAuthority = "Co quan quyet dinh",
            decisionPerson = (string?)null,
            totalInvestment = 1000m,
            stateCapital = 400m,
            centralBudget = 200m,
            localBudget = 100m,
            otherStateBudget = 100m,
            equityCapital = 300m,
            loanCapital = 300m,
            equityRatio = (decimal?)null,
            adjustmentContentId = (Guid?)null,
            notes = (string?)null,
            fileId = (Guid?)null,
        };
        var addResp = await client.PostAsJsonAsync($"{BaseUrl}/{projectId}/decisions", decisionBody);
        addResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var decisionId = (await addResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("data").GetString()!;

        // Delete decision
        var deleteResp = await client.DeleteAsync($"{BaseUrl}/{projectId}/decisions/{decisionId}");

        deleteResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── Sub-entities: Locations ───────────────────────────────────────────────

    [Fact]
    public async Task AddLocation_Returns200()
    {
        using var client = CreateWriteClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var createResp = await client.PostAsJsonAsync(BaseUrl, BuildCreateBody(suffix));
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var projectId = (await createResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("data").GetString()!;

        var locationBody = new
        {
            provinceId = Guid.NewGuid(),
            districtId = (Guid?)null,
            wardId = (Guid?)null,
            address = "123 Test Street",
        };

        var response = await client.PostAsJsonAsync($"{BaseUrl}/{projectId}/locations", locationBody);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteLocation_Returns200()
    {
        using var client = CreateWriteClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];

        // Create project
        var createResp = await client.PostAsJsonAsync(BaseUrl, BuildCreateBody(suffix));
        var projectId = (await createResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("data").GetString()!;

        // Add location
        var locationBody = new
        {
            provinceId = Guid.NewGuid(),
            districtId = (Guid?)null,
            wardId = (Guid?)null,
            address = "456 Delete St",
        };
        var addResp = await client.PostAsJsonAsync($"{BaseUrl}/{projectId}/locations", locationBody);
        addResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var locationId = (await addResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("data").GetString()!;

        // Delete
        var deleteResp = await client.DeleteAsync($"{BaseUrl}/{projectId}/locations/{locationId}");

        deleteResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── Sub-entities: Documents ───────────────────────────────────────────────

    [Fact]
    public async Task AddDocument_Returns200()
    {
        using var client = CreateWriteClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var createResp = await client.PostAsJsonAsync(BaseUrl, BuildCreateBody(suffix));
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var projectId = (await createResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("data").GetString()!;

        var documentBody = new
        {
            documentTypeId = Guid.NewGuid(),
            fileId = Guid.NewGuid(),
            title = $"Test Document {suffix}",
            notes = (string?)null,
        };

        var response = await client.PostAsJsonAsync($"{BaseUrl}/{projectId}/documents", documentBody);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteDocument_Returns200()
    {
        using var client = CreateWriteClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];

        // Create project
        var createResp = await client.PostAsJsonAsync(BaseUrl, BuildCreateBody(suffix));
        var projectId = (await createResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("data").GetString()!;

        // Add document
        var documentBody = new
        {
            documentTypeId = Guid.NewGuid(),
            fileId = Guid.NewGuid(),
            title = $"Doc to Delete {suffix}",
            notes = (string?)null,
        };
        var addResp = await client.PostAsJsonAsync($"{BaseUrl}/{projectId}/documents", documentBody);
        addResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var documentId = (await addResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("data").GetString()!;

        // Delete
        var deleteResp = await client.DeleteAsync($"{BaseUrl}/{projectId}/documents/{documentId}");

        deleteResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── Filtering & Pagination ────────────────────────────────────────────────

    [Fact]
    public async Task List_WithSearchFilter_Returns200()
    {
        using var client = CreateAuthenticatedClient(
            roles: ["BTC"], tenantId: DefaultTenantId.ToString());

        var response = await client.GetAsync($"{BaseUrl}?search=NONEXISTENT_XYZ_12345");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.TryGetProperty("data", out _).Should().BeTrue("response must wrap in ApiResponse envelope");
    }

    [Fact]
    public async Task List_Pagination_Returns200()
    {
        using var client = CreateAuthenticatedClient(
            roles: ["CQCQ"], tenantId: DefaultTenantId.ToString());

        var response = await client.GetAsync($"{BaseUrl}?page=1&pageSize=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
