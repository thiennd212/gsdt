using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace GSDT.Tests.Integration.Workflow;

/// <summary>
/// Integration tests for WorkflowDefinitionsController — /api/v1/workflow/definitions
///
/// Routes:
///   GET  /api/v1/workflow/definitions?tenantId={string}            → List (paged, any authenticated)
///   GET  /api/v1/workflow/definitions/{id}?tenantId={string}       → GetById (any authenticated)
///   POST /api/v1/workflow/definitions                              → Create [Admin,SystemAdmin]
///         body: CreateWorkflowDefinitionCommand {
///           Name, Description, TenantId (string), CreatedBy (Guid),
///           States: [ { Name, DisplayNameVi, DisplayNameEn, IsInitial, IsFinal, Color, SortOrder } ],
///           Transitions: [ { FromStateName, ToStateName, ActionName, ActionLabelVi, ActionLabelEn,
///                             RequiredRoleCode?, ConditionsJson?, SortOrder } ]
///         }
///
/// Note: TenantId is string (not Guid) in workflow definitions — matches GetWorkflowDefinitionsQuery.
/// </summary>
[Collection("Integration")]
public class WorkflowDefinitionsApiTests(DatabaseFixture db) : IntegrationTestBase(db)
{
    private const string BaseUrl = "/api/v1/workflow/definitions";

    // -----------------------------------------------------------------
    // LIST
    // -----------------------------------------------------------------

    [Fact]
    public async Task ListWorkflowDefinitions_AsAuthenticatedUser_Returns200()
    {
        var tenantId = Guid.NewGuid().ToString();
        var response = await Client.GetAsync($"{BaseUrl}?tenantId={tenantId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ListWorkflowDefinitions_ResponseBody_WrappedInApiResponseEnvelope()
    {
        var tenantId = Guid.NewGuid().ToString();
        var response = await Client.GetAsync($"{BaseUrl}?tenantId={tenantId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.TryGetProperty("data", out _).Should().BeTrue("response must be ApiResponse envelope");
    }

    [Fact]
    public async Task ListWorkflowDefinitions_Unauthenticated_Returns401Or403()
    {
        var anonClient = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var response = await anonClient.GetAsync($"{BaseUrl}?tenantId={Guid.NewGuid()}");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    // -----------------------------------------------------------------
    // CREATE
    // -----------------------------------------------------------------

    [Fact]
    public async Task CreateWorkflowDefinition_WithValidData_Returns200()
    {
        var tenantId = Guid.NewGuid().ToString();
        var createdBy = Guid.NewGuid();
        var adminClient = CreateAuthenticatedClient(
            userId: createdBy.ToString(),
            roles: ["SystemAdmin"],
            tenantId: tenantId);

        var response = await adminClient.PostAsJsonAsync(BaseUrl, new
        {
            Name = "Building Permit Approval Workflow",
            Description = "Standard workflow for building permit case approval",
            TenantId = tenantId,
            CreatedBy = createdBy,
            States = new[]
            {
                new
                {
                    Name = "Draft",
                    DisplayNameVi = "Nháp",
                    DisplayNameEn = "Draft",
                    IsInitial = true,
                    IsFinal = false,
                    Color = "#9E9E9E",
                    SortOrder = 1,
                },
                new
                {
                    Name = "Submitted",
                    DisplayNameVi = "Đã nộp",
                    DisplayNameEn = "Submitted",
                    IsInitial = false,
                    IsFinal = false,
                    Color = "#2196F3",
                    SortOrder = 2,
                },
                new
                {
                    Name = "Approved",
                    DisplayNameVi = "Đã duyệt",
                    DisplayNameEn = "Approved",
                    IsInitial = false,
                    IsFinal = true,
                    Color = "#4CAF50",
                    SortOrder = 3,
                },
            },
            Transitions = new[]
            {
                new
                {
                    FromStateName = "Draft",
                    ToStateName = "Submitted",
                    ActionName = "Submit",
                    ActionLabelVi = "Nộp hồ sơ",
                    ActionLabelEn = "Submit",
                    RequiredRoleCode = (string?)null,
                    ConditionsJson = (string?)null,
                    SortOrder = 1,
                },
                new
                {
                    FromStateName = "Submitted",
                    ToStateName = "Approved",
                    ActionName = "Approve",
                    ActionLabelVi = "Phê duyệt",
                    ActionLabelEn = "Approve",
                    RequiredRoleCode = (string?)"GovOfficer",
                    ConditionsJson = (string?)null,
                    SortOrder = 2,
                },
            },
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.TryGetProperty("data", out var data).Should().BeTrue();
        data.GetProperty("id").GetGuid().Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateWorkflowDefinition_WithoutAdminRole_Returns403()
    {
        var tenantId = Guid.NewGuid().ToString();
        var restrictedClient = CreateAuthenticatedClient(
            userId: Guid.NewGuid().ToString(),
            roles: ["CaseOfficer"],
            tenantId: tenantId);

        var response = await restrictedClient.PostAsJsonAsync(BaseUrl, new
        {
            Name = "Unauthorized Workflow",
            Description = "Should be rejected",
            TenantId = tenantId,
            CreatedBy = Guid.NewGuid(),
            States = Array.Empty<object>(),
            Transitions = Array.Empty<object>(),
        });

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    // -----------------------------------------------------------------
    // GET BY ID
    // -----------------------------------------------------------------

    [Fact]
    public async Task GetWorkflowDefinitionById_ForNonExistentId_Returns404()
    {
        var tenantId = Guid.NewGuid().ToString();
        var nonExistentId = Guid.NewGuid();

        var response = await Client.GetAsync($"{BaseUrl}/{nonExistentId}?tenantId={tenantId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetWorkflowDefinitionById_ForExistingDefinition_Returns200WithStatesAndTransitions()
    {
        var tenantId = Guid.NewGuid().ToString();
        var createdBy = Guid.NewGuid();
        var adminClient = CreateAuthenticatedClient(
            userId: createdBy.ToString(),
            roles: ["SystemAdmin"],
            tenantId: tenantId);

        // Create a definition
        var createResp = await adminClient.PostAsJsonAsync(BaseUrl, new
        {
            Name = "Complaint Review Workflow",
            Description = "Workflow for reviewing citizen complaints",
            TenantId = tenantId,
            CreatedBy = createdBy,
            States = new[]
            {
                new
                {
                    Name = "Open",
                    DisplayNameVi = "Mở",
                    DisplayNameEn = "Open",
                    IsInitial = true,
                    IsFinal = false,
                    Color = "#FF9800",
                    SortOrder = 1,
                },
                new
                {
                    Name = "Closed",
                    DisplayNameVi = "Đóng",
                    DisplayNameEn = "Closed",
                    IsInitial = false,
                    IsFinal = true,
                    Color = "#607D8B",
                    SortOrder = 2,
                },
            },
            Transitions = new[]
            {
                new
                {
                    FromStateName = "Open",
                    ToStateName = "Closed",
                    ActionName = "Close",
                    ActionLabelVi = "Đóng",
                    ActionLabelEn = "Close",
                    RequiredRoleCode = (string?)null,
                    ConditionsJson = (string?)null,
                    SortOrder = 1,
                },
            },
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.OK,
            "workflow definition creation must succeed before testing GetById");

        var createBody = await createResp.Content.ReadFromJsonAsync<JsonElement>();
        var definitionId = createBody.GetProperty("data").GetProperty("id").GetGuid();

        // GetById
        var getResp = await adminClient.GetAsync($"{BaseUrl}/{definitionId}?tenantId={tenantId}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await getResp.Content.ReadFromJsonAsync<JsonElement>();
        var data = body.GetProperty("data");
        data.GetProperty("id").GetGuid().Should().Be(definitionId);
        data.TryGetProperty("states", out _).Should().BeTrue("definition must include states");
        data.TryGetProperty("transitions", out _).Should().BeTrue("definition must include transitions");
    }

    // -----------------------------------------------------------------
    // FILTER: isActive query param
    // -----------------------------------------------------------------

    [Fact]
    public async Task ListWorkflowDefinitions_WithIsActiveFilter_Returns200()
    {
        var tenantId = Guid.NewGuid().ToString();
        var response = await Client.GetAsync($"{BaseUrl}?tenantId={tenantId}&isActive=true");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
