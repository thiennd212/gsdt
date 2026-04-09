using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace GSDT.Tests.Integration.InvestmentProjects;

/// <summary>
/// Regression tests — verify existing TN (domestic) and ODA project endpoints
/// still return 200 after adding NĐT and FDI project types in Phase 2.
/// Guards against routing conflicts and handler registration regressions.
/// Controllers require BTC,CQCQ,CDT for read and BTC,CDT for write.
/// Tests use role-specific clients — SystemAdmin is NOT in BTC/CQCQ/CDT.
/// </summary>
[Collection("Integration")]
public class TnOdaRegressionTests(DatabaseFixture db) : IntegrationTestBase(db)
{
    private const string DomesticBaseUrl = "/api/v1/domestic-projects";
    private const string OdaBaseUrl      = "/api/v1/oda-projects";

    // ── Shared client helpers ─────────────────────────────────────────────────

    /// <summary>Creates an HttpClient with BTC role for read operations.</summary>
    private HttpClient CreateReadClient() =>
        CreateAuthenticatedClient(roles: ["BTC"], tenantId: DefaultTenantId.ToString());

    /// <summary>Creates an HttpClient with BTC+CDT roles for write operations.</summary>
    private HttpClient CreateWriteClient() =>
        CreateAuthenticatedClient(roles: ["BTC", "CDT"], tenantId: DefaultTenantId.ToString());

    // ── Domestic (TN) helpers ─────────────────────────────────────────────────

    /// <summary>
    /// Minimal valid domestic create body.
    /// Handler computes: PrelimPublicInvestment = Central + Local + OtherPublic
    ///                   PrelimTotalInvestment  = PrelimPublicInvestment + OtherCapital
    /// So no total field in the request — server derives it.
    /// </summary>
    private static object ValidDomesticBody(string? projectCode = null) => new
    {
        projectCode             = projectCode ?? $"TN-{Guid.NewGuid():N}"[..20],
        projectName             = "Domestic Regression Project",
        managingAuthorityId     = Guid.NewGuid(),
        industrySectorId        = Guid.NewGuid(),
        projectOwnerId          = Guid.NewGuid(),
        projectGroupId          = Guid.NewGuid(),
        subProjectType          = 0,
        statusId                = Guid.NewGuid(),
        // Capital components — server computes totals
        prelimCentralBudget     = 40m,
        prelimLocalBudget       = 30m,
        prelimOtherPublicCapital = 10m,
        prelimOtherCapital      = 20m,
        // Optional fields
        projectManagementUnitId = (Guid?)null,
        pmuDirectorName         = (string?)null,
        pmuPhone                = (string?)null,
        pmuEmail                = (string?)null,
        implementationPeriod    = (string?)null,
        nationalTargetProgramId = (Guid?)null,
        treasuryCode            = (string?)null,
        policyDecisionNumber    = (string?)null,
        policyDecisionDate      = (DateTime?)null,
        policyDecisionAuthority = (string?)null,
        policyDecisionPerson    = (string?)null,
        policyDecisionFileId    = (Guid?)null,
    };

    // ── ODA helpers ───────────────────────────────────────────────────────────

    /// <summary>
    /// Minimal valid ODA create body.
    /// Handler computes: TotalInvestment = OdaGrant + OdaLoan + Central + Local + Other.
    /// ShortName, OdaProjectTypeId, DonorId are required.
    /// </summary>
    private static object ValidOdaBody(string? projectCode = null) => new
    {
        projectCode             = projectCode ?? $"ODA-{Guid.NewGuid():N}"[..20],
        projectName             = "ODA Regression Project",
        shortName               = "ODA-REG",
        managingAuthorityId     = Guid.NewGuid(),
        industrySectorId        = Guid.NewGuid(),
        projectOwnerId          = Guid.NewGuid(),
        odaProjectTypeId        = Guid.NewGuid(),
        donorId                 = Guid.NewGuid(),
        statusId                = Guid.NewGuid(),
        // ODA capital breakdown — server computes TotalInvestment
        odaGrantCapital           = 60m,
        odaLoanCapital            = 0m,
        counterpartCentralBudget  = 25m,
        counterpartLocalBudget    = 10m,
        counterpartOtherCapital   = 5m,
        // Mechanism percentages (0–100)
        grantMechanismPercent     = 100m,
        relendingMechanismPercent = 0m,
        // Classification
        procurementConditionBound   = false,
        procurementConditionSummary = (string?)null,
        startYear                   = (int?)null,
        endYear                     = (int?)null,
        // Optional
        projectManagementUnitId = (Guid?)null,
        pmuDirectorName         = (string?)null,
        pmuPhone                = (string?)null,
        pmuEmail                = (string?)null,
        implementationPeriod    = (string?)null,
        projectCodeQhns         = (string?)null,
        coDonorName             = (string?)null,
        policyDecisionNumber    = (string?)null,
        policyDecisionDate      = (DateTime?)null,
        policyDecisionAuthority = (string?)null,
        policyDecisionPerson    = (string?)null,
        policyDecisionFileId    = (Guid?)null,
    };

    // ── Domestic (TN) regression tests ───────────────────────────────────────

    [Fact]
    public async Task DomesticProjects_List_Returns200()
    {
        using var client = CreateReadClient();
        var response = await client.GetAsync(DomesticBaseUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.TryGetProperty("data", out _).Should().BeTrue("response must be wrapped in ApiResponse envelope");
    }

    [Fact]
    public async Task DomesticProjects_Create_Returns200()
    {
        using var client = CreateWriteClient();
        var response = await client.PostAsJsonAsync(DomesticBaseUrl, ValidDomesticBody());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.TryGetProperty("data", out var data).Should().BeTrue();
        // Returned data is the new project Guid
        data.GetGuid().Should().NotBeEmpty();
    }

    // ── ODA regression tests ──────────────────────────────────────────────────

    [Fact]
    public async Task OdaProjects_List_Returns200()
    {
        using var client = CreateReadClient();
        var response = await client.GetAsync(OdaBaseUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.TryGetProperty("data", out _).Should().BeTrue("response must be wrapped in ApiResponse envelope");
    }

    [Fact]
    public async Task OdaProjects_Create_Returns200()
    {
        using var client = CreateWriteClient();
        var response = await client.PostAsJsonAsync(OdaBaseUrl, ValidOdaBody());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.TryGetProperty("data", out var data).Should().BeTrue();
        // Returned data is the new project Guid
        data.GetGuid().Should().NotBeEmpty();
    }
}
