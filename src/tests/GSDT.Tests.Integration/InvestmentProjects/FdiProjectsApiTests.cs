using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace GSDT.Tests.Integration.InvestmentProjects;

/// <summary>
/// Integration tests for FdiProjectsController — /api/v1/fdi-projects
/// FDI = Foreign Direct Investment.
/// Capital rule: PrelimTotalInvestment == PrelimEquityCapital + PrelimOdaLoanCapital + PrelimCreditLoanCapital
/// Roles required: BTC,CDT for write; BTC,CQCQ,CDT for read.
/// Default Client is pre-authenticated as SystemAdmin.
/// </summary>
[Collection("Integration")]
public class FdiProjectsApiTests(DatabaseFixture db) : IntegrationTestBase(db)
{
    private const string BaseUrl = "/api/v1/fdi-projects";

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Minimal valid FDI create body. Capital: 150 = 60 + 50 + 40.</summary>
    private static object ValidCreateBody(string? projectCode = null) => new
    {
        projectCode         = projectCode ?? $"FDI-{Guid.NewGuid():N}"[..20],
        projectName         = "FDI Test Project",
        managingAuthorityId = Guid.NewGuid(),
        industrySectorId    = Guid.NewGuid(),
        projectOwnerId      = Guid.NewGuid(),
        projectGroupId      = Guid.NewGuid(),
        subProjectType      = 0,
        statusId            = Guid.NewGuid(),
        // Capital breakdown — must balance: total == equity + oda + credit
        prelimTotalInvestment   = 150m,
        prelimEquityCapital     = 60m,
        prelimOdaLoanCapital    = 50m,
        prelimCreditLoanCapital = 40m,
    };

    /// <summary>Creates a project and returns its Guid id.</summary>
    private async Task<Guid> CreateProjectAsync(string? code = null)
    {
        var response = await Client.PostAsJsonAsync(BaseUrl, ValidCreateBody(code));
        response.StatusCode.Should().Be(HttpStatusCode.OK, "pre-condition: project creation must succeed");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("data").GetGuid();
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
    public async Task Create_ValidProject_Returns200()
    {
        var response = await Client.PostAsJsonAsync(BaseUrl, ValidCreateBody());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.TryGetProperty("data", out var data).Should().BeTrue();
        // Returned data is the new project Guid
        data.GetGuid().Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetById_Returns200()
    {
        var id = await CreateProjectAsync();

        var response = await Client.GetAsync($"{BaseUrl}/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("data").TryGetProperty("id", out var idProp).Should().BeTrue();
        idProp.GetGuid().Should().Be(id);
    }

    [Fact]
    public async Task Update_Returns200()
    {
        var id = await CreateProjectAsync();

        // Fetch RowVersion for optimistic concurrency
        var getResponse = await Client.GetAsync($"{BaseUrl}/{id}");
        var getBody = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
        var rowVersion = getBody.GetProperty("data").GetProperty("rowVersion").GetBytesFromBase64();

        var updateBody = new
        {
            id,
            rowVersion,
            projectCode         = $"FDI-UPD-{Guid.NewGuid():N}"[..20],
            projectName         = "FDI Updated Name",
            managingAuthorityId = Guid.NewGuid(),
            industrySectorId    = Guid.NewGuid(),
            projectOwnerId      = Guid.NewGuid(),
            projectGroupId      = Guid.NewGuid(),
            subProjectType      = 0,
            statusId            = Guid.NewGuid(),
            // Capital must still balance: 300 == 120 + 100 + 80
            prelimTotalInvestment   = 300m,
            prelimEquityCapital     = 120m,
            prelimOdaLoanCapital    = 100m,
            prelimCreditLoanCapital = 80m,
        };

        var response = await Client.PutAsJsonAsync($"{BaseUrl}/{id}", updateBody);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_Returns200()
    {
        var id = await CreateProjectAsync();

        var response = await Client.DeleteAsync($"{BaseUrl}/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AddDecision_Returns200()
    {
        var id = await CreateProjectAsync();

        // Decision capital must also balance: 150 == 60 + 50 + 40
        var decisionBody = new
        {
            projectId          = id,
            decisionType       = 1,
            decisionNumber     = $"QĐ-FDI-{Guid.NewGuid():N}"[..20],
            decisionDate       = DateTime.UtcNow.Date,
            decisionAuthority  = "Bộ Kế hoạch và Đầu tư",
            decisionPerson     = (string?)null,
            totalInvestment    = 150m,
            equityCapital      = 60m,
            odaLoanCapital     = 50m,
            creditLoanCapital  = 40m,
            equityRatio        = (decimal?)null,
            adjustmentContentId = (Guid?)null,
            notes              = (string?)null,
            fileId             = (Guid?)null,
        };

        var response = await Client.PostAsJsonAsync($"{BaseUrl}/{id}/decisions", decisionBody);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AddCertificate_Returns200()
    {
        var id = await CreateProjectAsync();

        var certBody = new
        {
            projectId         = id,
            certificateNumber = $"GCNĐKĐT-FDI-{Guid.NewGuid():N}"[..25],
            issuedDate        = DateTime.UtcNow.Date,
            investmentCapital = 150m,
            equityCapital     = 60m,
            equityRatio       = (decimal?)null,
            notes             = (string?)null,
            fileId            = (Guid?)null,
        };

        var response = await Client.PostAsJsonAsync($"{BaseUrl}/{id}/certificates", certBody);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AddLocation_Returns200()
    {
        var id = await CreateProjectAsync();

        var locationBody = new
        {
            projectId  = id,
            provinceId = Guid.NewGuid(),
            districtId = (Guid?)null,
            wardId     = (Guid?)null,
            address    = (string?)null,
        };

        var response = await Client.PostAsJsonAsync($"{BaseUrl}/{id}/locations", locationBody);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
