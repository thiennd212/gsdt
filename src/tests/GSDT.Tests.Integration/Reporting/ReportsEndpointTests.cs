using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace GSDT.Tests.Integration.Reporting;

/// <summary>
/// Integration tests for ReportsController — /api/v1/reports
/// Routes exercised:
///   GET  /api/v1/reports/dashboard
///   GET  /api/v1/reports/definitions
///   POST /api/v1/reports/definitions
///   POST /api/v1/reports/run
///   GET  /api/v1/reports/executions/{id}
/// </summary>
[Collection("Integration")]
public class ReportsEndpointTests(DatabaseFixture db) : IntegrationTestBase(db)
{
    private const string DashboardUrl = "/api/v1/reports/dashboard";
    private const string DefinitionsUrl = "/api/v1/reports/definitions";
    private const string RunUrl = "/api/v1/reports/run";
    private const string ExecutionsUrl = "/api/v1/reports/executions";

    // Valid SQL template: starts with SELECT, contains @TenantId, no dangerous keywords
    private const string ValidSqlTemplate =
        "SELECT Id, Title, Status FROM cases.Cases WHERE TenantId = @TenantId AND IsDeleted = 0";

    // Minimal valid ParametersSchema (empty JSON object — validator only checks NotEmpty)
    private const string ValidParametersSchema = "{}";

    // -----------------------------------------------------------------------
    // 1. GetDashboard_Authenticated_Returns200
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetDashboard_Authenticated_Returns200()
    {
        // The KPI query filters cases.Cases WHERE TenantId = @TenantId (uniqueidentifier).
        // Must pass a valid Guid via X-Test-TenantId to avoid SQL Server conversion error.
        var tenantId = Guid.NewGuid().ToString();
        using var client = CreateAuthenticatedClient(
            roles: ["SystemAdmin"],
            tenantId: tenantId);

        var response = await client.GetAsync(DashboardUrl);

        // Counts are all 0 for an unknown tenant — but the query itself succeeds.
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.TryGetProperty("data", out _).Should().BeTrue("response must be ApiResponse envelope");
    }

    // -----------------------------------------------------------------------
    // 2. ListDefinitions_Authenticated_Returns200
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ListDefinitions_Authenticated_Returns200()
    {
        var response = await Client.GetAsync(DefinitionsUrl);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.TryGetProperty("data", out _).Should().BeTrue("response must be ApiResponse envelope");
    }

    // -----------------------------------------------------------------------
    // 3. CreateDefinition_AdminRole_Returns200
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CreateDefinition_AdminRole_Returns200WithGuid()
    {
        using var client = CreateAuthenticatedClient(
            userId: Guid.NewGuid().ToString(),
            roles: ["Admin"]);

        var response = await client.PostAsJsonAsync(DefinitionsUrl, new
        {
            Name = "Cases Summary Report",
            NameVi = "Bao cao tong hop ho so",
            Description = "Counts cases grouped by status for a given tenant.",
            SqlTemplate = ValidSqlTemplate,
            ParametersSchema = ValidParametersSchema,
            OutputFormat = 1, // OutputFormat.Excel
            TenantId = (string?)null,
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var definitionId = body.GetProperty("data").GetGuid();
        definitionId.Should().NotBe(Guid.Empty);
    }

    // -----------------------------------------------------------------------
    // 4. CreateDefinition_DangerousSql_Returns422
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CreateDefinition_DangerousSql_Returns422()
    {
        // SQL contains DROP TABLE — blocked by SqlValidationHelper in the command handler.
        // DangerousSqlError extends ValidationError so ApiControllerBase maps it to 422.
        using var client = CreateAuthenticatedClient(
            userId: Guid.NewGuid().ToString(),
            roles: ["Admin"]);

        var response = await client.PostAsJsonAsync(DefinitionsUrl, new
        {
            Name = "Bad Report",
            NameVi = "Bao cao loi",
            Description = "Report definition with dangerous SQL.",
            SqlTemplate = "SELECT * FROM cases.Cases WHERE TenantId = @TenantId; DROP TABLE cases.Cases --",
            ParametersSchema = ValidParametersSchema,
            OutputFormat = 1,
            TenantId = (string?)null,
        });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    // -----------------------------------------------------------------------
    // 5. RunReport_ValidDefinition_Returns202
    // -----------------------------------------------------------------------

    [Fact]
    public async Task RunReport_ValidDefinition_Returns202WithExecutionId()
    {
        var tenantId = Guid.NewGuid().ToString();

        using var adminClient = CreateAuthenticatedClient(
            userId: Guid.NewGuid().ToString(),
            roles: ["Admin"],
            tenantId: tenantId);

        // First create a report definition so RunReport can find it
        var createResp = await adminClient.PostAsJsonAsync(DefinitionsUrl, new
        {
            Name = "Run Test Report",
            NameVi = "Bao cao chay thu",
            Description = "Report definition used for RunReport integration test.",
            SqlTemplate = ValidSqlTemplate,
            ParametersSchema = ValidParametersSchema,
            OutputFormat = 1,
            TenantId = (string?)null,
        });

        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var createBody = await createResp.Content.ReadFromJsonAsync<JsonElement>();
        var definitionId = createBody.GetProperty("data").GetGuid();

        // Run the report — should return 202 Accepted with executionId in body
        var runResp = await adminClient.PostAsJsonAsync(RunUrl, new
        {
            ReportDefinitionId = definitionId,
            ParametersJson = "{}",
            FormatOverride = (object?)null,
        });

        runResp.StatusCode.Should().Be(HttpStatusCode.Accepted);

        // Body is the raw executionId Guid returned by AcceptedAtAction
        var executionId = await runResp.Content.ReadFromJsonAsync<Guid>();
        executionId.Should().NotBe(Guid.Empty);
    }

    // -----------------------------------------------------------------------
    // 6. GetExecution_NonExistent_Returns404
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetExecution_NonExistent_Returns404()
    {
        var nonExistentId = Guid.NewGuid();

        // SystemAdmin bypasses tenant isolation (requestedBy = null in handler),
        // so this cleanly tests the not-found path without role complications.
        var response = await Client.GetAsync($"{ExecutionsUrl}/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
