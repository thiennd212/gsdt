using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace GSDT.Tests.Integration.InvestmentProjects;

/// <summary>
/// Integration tests for DnnnProjectsController — /api/v1/dnnn-projects.
/// Client is pre-authenticated as SystemAdmin with tenantId 00000000-0000-0000-0000-000000000001.
/// Write tests use a BTC+CDT client to satisfy [Authorize(Roles = "BTC,CDT")].
/// Capital rule enforced at validator layer: PrelimTotalInvestment == CSH + ODA + TCTD.
/// </summary>
[Collection("Integration")]
public class DnnnProjectsApiTests(DatabaseFixture db) : IntegrationTestBase(db)
{
    private const string BaseUrl = "/api/v1/dnnn-projects";

    // ── Helpers ──────────────────────────────────────────────────────────────

    private HttpClient CreateWriteClient() =>
        CreateAuthenticatedClient(roles: ["BTC", "CDT"], tenantId: DefaultTenantId.ToString());

    /// <summary>
    /// Returns a minimal valid DNNN create body.
    /// Capital constraint: total == equity + oda + credit.
    /// </summary>
    private static object BuildCreateBody(string suffix) => new
    {
        projectCode = $"DNNN-{suffix}",
        projectName = $"DNNN Test Project {suffix}",
        managingAuthorityId = Guid.NewGuid(),
        industrySectorId = Guid.NewGuid(),
        projectOwnerId = Guid.NewGuid(),
        projectGroupId = Guid.NewGuid(),
        subProjectType = 0,
        statusId = Guid.NewGuid(),
        // Capital: 1000 == 400 + 300 + 300 (equity + oda + credit)
        prelimTotalInvestment = 1000m,
        prelimEquityCapital = 400m,
        prelimOdaLoanCapital = 300m,
        prelimCreditLoanCapital = 300m,
        // Optional fields
        competentAuthorityId = (Guid?)null,
        investorName = (string?)null,
        stateOwnershipRatio = (decimal?)null,
        objective = (string?)null,
        areaHectares = (decimal?)null,
        capacity = (string?)null,
        mainItems = (string?)null,
        implementationTimeline = (string?)null,
        progressDescription = (string?)null,
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
    public async Task List_Returns200()
    {
        using var client = CreateAuthenticatedClient(
            roles: ["BTC"], tenantId: DefaultTenantId.ToString());

        var response = await client.GetAsync(BaseUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Create_ValidProject_Returns200WithId()
    {
        using var client = CreateWriteClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var response = await client.PostAsJsonAsync(BaseUrl, BuildCreateBody(suffix));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.TryGetProperty("data", out var data).Should().BeTrue("ApiResponse must have 'data'");
        data.ValueKind.Should().Be(JsonValueKind.String);
        Guid.TryParse(data.GetString(), out _).Should().BeTrue("data should be a valid Guid");
    }

    [Fact]
    public async Task GetById_Returns200()
    {
        using var client = CreateWriteClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var expectedCode = $"DNNN-{suffix}";

        // Create
        var createResp = await client.PostAsJsonAsync(BaseUrl, BuildCreateBody(suffix));
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var newId = (await createResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("data").GetString()!;

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
    public async Task Update_Returns200()
    {
        using var client = CreateWriteClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];

        // Step 1: create
        var createResp = await client.PostAsJsonAsync(BaseUrl, BuildCreateBody(suffix));
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var newId = (await createResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("data").GetString()!;

        // Step 2: get rowVersion
        using var readClient = CreateAuthenticatedClient(
            roles: ["BTC"], tenantId: DefaultTenantId.ToString());
        var getResp = await readClient.GetAsync($"{BaseUrl}/{newId}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var getJson = await getResp.Content.ReadFromJsonAsync<JsonElement>();
        var rowVersionBase64 = getJson.GetProperty("data").GetProperty("rowVersion").GetString()!;
        var rowVersionBytes = Convert.FromBase64String(rowVersionBase64);

        // Step 3: update with balanced capital (2000 == 800 + 600 + 600)
        var updateBody = new
        {
            id = Guid.Parse(newId),
            rowVersion = rowVersionBytes,
            projectCode = $"DNNN-{suffix}",
            projectName = $"DNNN Updated {suffix}",
            managingAuthorityId = Guid.NewGuid(),
            industrySectorId = Guid.NewGuid(),
            projectOwnerId = Guid.NewGuid(),
            projectGroupId = Guid.NewGuid(),
            subProjectType = 0,
            statusId = Guid.NewGuid(),
            prelimTotalInvestment = 2000m,
            prelimEquityCapital = 800m,
            prelimOdaLoanCapital = 600m,
            prelimCreditLoanCapital = 600m,
            competentAuthorityId = (Guid?)null,
            investorName = (string?)null,
            stateOwnershipRatio = (decimal?)null,
            objective = (string?)null,
            areaHectares = (decimal?)null,
            capacity = (string?)null,
            mainItems = (string?)null,
            implementationTimeline = (string?)null,
            progressDescription = (string?)null,
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

        putResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_Returns200()
    {
        using var client = CreateWriteClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var createResp = await client.PostAsJsonAsync(BaseUrl, BuildCreateBody(suffix));
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var newId = (await createResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("data").GetString()!;

        var deleteResp = await client.DeleteAsync($"{BaseUrl}/{newId}");

        deleteResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    // ── Validation ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_CapitalMismatch_Returns400()
    {
        using var client = CreateWriteClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];

        // Deliberately break: total=1000 but equity+oda+credit=400+300+200=900 (mismatch)
        var invalidBody = new
        {
            projectCode = $"DNNN-BAD-{suffix}",
            projectName = $"DNNN Capital Mismatch {suffix}",
            managingAuthorityId = Guid.NewGuid(),
            industrySectorId = Guid.NewGuid(),
            projectOwnerId = Guid.NewGuid(),
            projectGroupId = Guid.NewGuid(),
            subProjectType = 0,
            statusId = Guid.NewGuid(),
            prelimTotalInvestment = 1000m,     // total declared as 1000
            prelimEquityCapital = 400m,
            prelimOdaLoanCapital = 300m,
            prelimCreditLoanCapital = 200m,    // 400+300+200=900 ≠ 1000 → validation error
        };

        var response = await client.PostAsJsonAsync(BaseUrl, invalidBody);

        // Validator returns ValidationError → UnprocessableEntity (422) via ToApiResponse.
        // ASP.NET model binding failures return BadRequest (400). Accept both.
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    // ── Sub-entities: Decisions ───────────────────────────────────────────────

    [Fact]
    public async Task AddDecision_Returns200()
    {
        using var client = CreateWriteClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var createResp = await client.PostAsJsonAsync(BaseUrl, BuildCreateBody(suffix));
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var projectId = (await createResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("data").GetString()!;

        // DNNN decision capital: Total == EquityCapital + OdaLoanCapital + CreditLoanCapital
        var decisionBody = new
        {
            decisionType = 1,
            decisionNumber = $"QD-DNNN-{suffix}",
            decisionDate = DateTime.UtcNow.Date,
            decisionAuthority = "Bo Ke Hoach va Dau Tu",
            decisionPerson = (string?)null,
            totalInvestment = 1000m,
            equityCapital = 400m,
            odaLoanCapital = 300m,
            creditLoanCapital = 300m,
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
        var projectId = (await createResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("data").GetString()!;

        // Add decision
        var decisionBody = new
        {
            decisionType = 1,
            decisionNumber = $"QD-DNNN-DEL-{suffix}",
            decisionDate = DateTime.UtcNow.Date,
            decisionAuthority = "Co quan quyet dinh",
            decisionPerson = (string?)null,
            totalInvestment = 1000m,
            equityCapital = 400m,
            odaLoanCapital = 300m,
            creditLoanCapital = 300m,
            equityRatio = (decimal?)null,
            adjustmentContentId = (Guid?)null,
            notes = (string?)null,
            fileId = (Guid?)null,
        };
        var addResp = await client.PostAsJsonAsync($"{BaseUrl}/{projectId}/decisions", decisionBody);
        addResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var decisionId = (await addResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("data").GetString()!;

        // Delete
        var deleteResp = await client.DeleteAsync($"{BaseUrl}/{projectId}/decisions/{decisionId}");

        deleteResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
    }

    // ── Sub-entities: Certificates ────────────────────────────────────────────

    [Fact]
    public async Task AddCertificate_Returns200()
    {
        using var client = CreateWriteClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var createResp = await client.PostAsJsonAsync(BaseUrl, BuildCreateBody(suffix));
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var projectId = (await createResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("data").GetString()!;

        var certBody = new
        {
            certificateNumber = $"GCNĐKĐT-{suffix}",
            issuedDate = DateTime.UtcNow.Date,
            investmentCapital = 1000m,
            equityCapital = 400m,
            equityRatio = (decimal?)40m,
            notes = (string?)null,
            fileId = (Guid?)null,
        };

        var response = await client.PostAsJsonAsync($"{BaseUrl}/{projectId}/certificates", certBody);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteCertificate_Returns200()
    {
        using var client = CreateWriteClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];

        // Create project
        var createResp = await client.PostAsJsonAsync(BaseUrl, BuildCreateBody(suffix));
        var projectId = (await createResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("data").GetString()!;

        // Add certificate
        var certBody = new
        {
            certificateNumber = $"GCNĐKĐT-DEL-{suffix}",
            issuedDate = DateTime.UtcNow.Date,
            investmentCapital = 500m,
            equityCapital = 200m,
            equityRatio = (decimal?)null,
            notes = (string?)null,
            fileId = (Guid?)null,
        };
        var addResp = await client.PostAsJsonAsync($"{BaseUrl}/{projectId}/certificates", certBody);
        addResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var certId = (await addResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("data").GetString()!;

        // Delete
        var deleteResp = await client.DeleteAsync($"{BaseUrl}/{projectId}/certificates/{certId}");

        deleteResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);
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
            address = $"789 DNNN Test St {suffix}",
        };

        var response = await client.PostAsJsonAsync($"{BaseUrl}/{projectId}/locations", locationBody);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── Filtering ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task List_WithFilters_Returns200()
    {
        using var client = CreateAuthenticatedClient(
            roles: ["BTC"], tenantId: DefaultTenantId.ToString());

        // Test all available query parameters render without error
        var response = await client.GetAsync(
            $"{BaseUrl}?page=1&pageSize=10&search=NONEXISTENT&investorName=TestInvestor");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.TryGetProperty("data", out _).Should().BeTrue("response must wrap in ApiResponse envelope");
    }
}
