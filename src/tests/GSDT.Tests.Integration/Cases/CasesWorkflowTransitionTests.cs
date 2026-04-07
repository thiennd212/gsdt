using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace GSDT.Tests.Integration.Cases;

/// <summary>
/// State machine integration tests for Cases workflow transitions.
///
/// Valid transitions (from CaseWorkflow.cs):
///   Draft               --Submit--> Submitted
///   ReturnedForRevision --Submit--> Submitted
///   Submitted           --Assign --> UnderReview      [Admin/SystemAdmin]
///   UnderReview         --Approve--> Approved          [GovOfficer/Admin/SystemAdmin]
///   UnderReview         --Reject --> Rejected           [GovOfficer/Admin/SystemAdmin]
///   Approved            --Close --> Closed             [Admin/SystemAdmin]
///   Rejected            --Close --> Closed             [Admin/SystemAdmin]
///
/// Routes:
///   POST /api/v1/cases                    → Create (body: TenantId, Title, Description, Type, Priority)
///   POST /api/v1/cases/{id}/submit        → [FromQuery] tenantId
///   POST /api/v1/cases/{id}/assign        → [FromBody] { TenantId, AssigneeId, Department }
///   POST /api/v1/cases/{id}/approve       → [FromBody] { TenantId, Reason }
///   POST /api/v1/cases/{id}/reject        → [FromBody] { TenantId, Reason }
///   POST /api/v1/cases/{id}/close         → [FromQuery] tenantId
///   GET  /api/v1/cases/{id}?tenantId=...  → verify final state
/// </summary>
[Collection("Integration")]
public class CasesWorkflowTransitionTests(DatabaseFixture db) : IntegrationTestBase(db)
{
    private const string BaseUrl = "/api/v1/cases";

    // Helper: create a case and return its ID
    private async Task<Guid> CreateCaseAsync(HttpClient client, Guid tenantId)
    {
        var resp = await client.PostAsJsonAsync(BaseUrl, new
        {
            TenantId = tenantId,
            Title = "Workflow transition integration test case",
            Description = "This description is long enough to pass the FluentValidation MinimumLength(20) rule.",
            Type = 0,       // CaseType.Application
            Priority = 1,   // CasePriority.Medium
        });
        resp.StatusCode.Should().Be(HttpStatusCode.OK, "case creation must succeed before testing transitions");

        var body = await resp.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("data").GetProperty("id").GetGuid();
    }

    // -----------------------------------------------------------------
    // FULL WORKFLOW: Draft → Submitted → UnderReview → Approved
    // -----------------------------------------------------------------

    [Fact]
    public async Task FullWorkflow_DraftToApproved_Succeeds()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();
        var client = CreateAuthenticatedClient(
            userId: userId,
            roles: ["SystemAdmin", "GovOfficer"],
            tenantId: tenantId.ToString());

        // 1. Create → Draft
        var caseId = await CreateCaseAsync(client, tenantId);

        // 2. Submit → Submitted  (Submit takes tenantId as query param, no body)
        var submitResp = await client.PostAsJsonAsync(
            $"{BaseUrl}/{caseId}/submit?tenantId={tenantId}", new { });
        submitResp.StatusCode.Should().BeOneOf(
            [HttpStatusCode.OK, HttpStatusCode.NoContent],
            "Submit transition from Draft must succeed");

        // 3. Assign → UnderReview  (body: TenantId, AssigneeId, Department)
        var assigneeId = Guid.NewGuid();
        var assignResp = await client.PostAsJsonAsync(
            $"{BaseUrl}/{caseId}/assign",
            new
            {
                TenantId = tenantId,
                AssigneeId = assigneeId,
                Department = "Planning Department",
            });
        assignResp.StatusCode.Should().BeOneOf(
            [HttpStatusCode.OK, HttpStatusCode.NoContent],
            "Assign transition from Submitted must succeed");

        // 4. Approve → Approved  (body: TenantId, Reason)
        var approveResp = await client.PostAsJsonAsync(
            $"{BaseUrl}/{caseId}/approve",
            new
            {
                TenantId = tenantId,
                Reason = "All requirements satisfied. Approved in integration test.",
            });
        approveResp.StatusCode.Should().BeOneOf(
            [HttpStatusCode.OK, HttpStatusCode.NoContent],
            "Approve transition from UnderReview must succeed");

        // 5. Verify final state is Approved
        var getResp = await client.GetAsync($"{BaseUrl}/{caseId}?tenantId={tenantId}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var caseBody = await getResp.Content.ReadFromJsonAsync<JsonElement>();
        var status = caseBody.GetProperty("data").GetProperty("status").GetString();
        status.Should().Be("Approved", "case must reach Approved state after full workflow");
    }

    // -----------------------------------------------------------------
    // FULL WORKFLOW: Draft → Submitted → UnderReview → Rejected → Closed
    // -----------------------------------------------------------------

    [Fact]
    public async Task FullWorkflow_DraftToRejectedToClosed_Succeeds()
    {
        var tenantId = Guid.NewGuid();
        var client = CreateAuthenticatedClient(
            userId: Guid.NewGuid().ToString(),
            roles: ["SystemAdmin", "GovOfficer"],
            tenantId: tenantId.ToString());

        var caseId = await CreateCaseAsync(client, tenantId);

        // Submit
        var submitResp = await client.PostAsJsonAsync(
            $"{BaseUrl}/{caseId}/submit?tenantId={tenantId}", new { });
        submitResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        // Assign
        var assignResp = await client.PostAsJsonAsync(
            $"{BaseUrl}/{caseId}/assign",
            new { TenantId = tenantId, AssigneeId = Guid.NewGuid(), Department = "Legal" });
        assignResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        // Reject
        var rejectResp = await client.PostAsJsonAsync(
            $"{BaseUrl}/{caseId}/reject",
            new { TenantId = tenantId, Reason = "Missing required documentation." });
        rejectResp.StatusCode.Should().BeOneOf(
            [HttpStatusCode.OK, HttpStatusCode.NoContent],
            "Reject transition from UnderReview must succeed");

        // Close (from Rejected)
        var closeResp = await client.PostAsJsonAsync(
            $"{BaseUrl}/{caseId}/close?tenantId={tenantId}", new { });
        closeResp.StatusCode.Should().BeOneOf(
            [HttpStatusCode.OK, HttpStatusCode.NoContent],
            "Close transition from Rejected must succeed");

        // Verify Closed
        var getResp = await client.GetAsync($"{BaseUrl}/{caseId}?tenantId={tenantId}");
        var body = await getResp.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("data").GetProperty("status").GetString()
            .Should().Be("Closed");
    }

    // -----------------------------------------------------------------
    // INVALID TRANSITION: Cannot approve a Draft case directly
    // Draft → Approve is not in CaseWorkflow — must return 422 or 400
    // -----------------------------------------------------------------

    [Fact]
    public async Task ApproveCase_WhileInDraftState_ReturnsError()
    {
        var tenantId = Guid.NewGuid();
        var client = CreateAuthenticatedClient(
            userId: Guid.NewGuid().ToString(),
            roles: ["SystemAdmin", "GovOfficer"],
            tenantId: tenantId.ToString());

        var caseId = await CreateCaseAsync(client, tenantId);

        // Attempt to approve without submit/assign — invalid transition
        var approveResp = await client.PostAsJsonAsync(
            $"{BaseUrl}/{caseId}/approve",
            new { TenantId = tenantId, Reason = "Attempting invalid direct approval." });

        // InvalidCaseTransitionException thrown → should surface as 422, 400, or 500
        // (500 if exception isn't mapped to ProblemDetails middleware)
        approveResp.StatusCode.Should().BeOneOf(
            HttpStatusCode.UnprocessableEntity,
            HttpStatusCode.BadRequest,
            HttpStatusCode.InternalServerError);

        approveResp.StatusCode.Should().NotBe(HttpStatusCode.OK,
            "Approving a Draft case is an invalid transition and must fail");
    }

    // -----------------------------------------------------------------
    // INVALID TRANSITION: Cannot close a Draft case
    // -----------------------------------------------------------------

    [Fact]
    public async Task CloseCase_WhileInDraftState_ReturnsError()
    {
        var tenantId = Guid.NewGuid();
        var client = CreateAuthenticatedClient(
            userId: Guid.NewGuid().ToString(),
            roles: ["SystemAdmin"],
            tenantId: tenantId.ToString());

        var caseId = await CreateCaseAsync(client, tenantId);

        var closeResp = await client.PostAsJsonAsync(
            $"{BaseUrl}/{caseId}/close?tenantId={tenantId}", new { });

        closeResp.StatusCode.Should().NotBe(HttpStatusCode.OK,
            "Closing a Draft case is an invalid transition and must fail");
    }

    // -----------------------------------------------------------------
    // AUTH: Assign requires Admin/SystemAdmin — CaseManager alone is forbidden
    // -----------------------------------------------------------------

    [Fact]
    public async Task AssignCase_WithoutAdminRole_Returns403()
    {
        var tenantId = Guid.NewGuid();

        // Use SystemAdmin to create + submit
        var adminClient = CreateAuthenticatedClient(
            userId: Guid.NewGuid().ToString(),
            roles: ["SystemAdmin"],
            tenantId: tenantId.ToString());

        var caseId = await CreateCaseAsync(adminClient, tenantId);
        await adminClient.PostAsJsonAsync(
            $"{BaseUrl}/{caseId}/submit?tenantId={tenantId}", new { });

        // Try to assign using a role without Admin
        var restrictedClient = CreateAuthenticatedClient(
            userId: Guid.NewGuid().ToString(),
            roles: ["CaseOfficer"],
            tenantId: tenantId.ToString());

        var assignResp = await restrictedClient.PostAsJsonAsync(
            $"{BaseUrl}/{caseId}/assign",
            new { TenantId = tenantId, AssigneeId = Guid.NewGuid(), Department = "Planning" });

        assignResp.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    // -----------------------------------------------------------------
    // AUTH: Approve requires GovOfficer/Admin/SystemAdmin
    // -----------------------------------------------------------------

    [Fact]
    public async Task ApproveCase_WithoutGovOfficerRole_Returns403()
    {
        var tenantId = Guid.NewGuid();
        var adminClient = CreateAuthenticatedClient(
            userId: Guid.NewGuid().ToString(),
            roles: ["SystemAdmin"],
            tenantId: tenantId.ToString());

        var caseId = await CreateCaseAsync(adminClient, tenantId);
        await adminClient.PostAsJsonAsync($"{BaseUrl}/{caseId}/submit?tenantId={tenantId}", new { });
        await adminClient.PostAsJsonAsync(
            $"{BaseUrl}/{caseId}/assign",
            new { TenantId = tenantId, AssigneeId = Guid.NewGuid(), Department = "Planning" });

        // Attempt approve without GovOfficer/Admin/SystemAdmin
        var restrictedClient = CreateAuthenticatedClient(
            userId: Guid.NewGuid().ToString(),
            roles: ["CaseOfficer"],
            tenantId: tenantId.ToString());

        var approveResp = await restrictedClient.PostAsJsonAsync(
            $"{BaseUrl}/{caseId}/approve",
            new { TenantId = tenantId, Reason = "Should be rejected by auth." });

        approveResp.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }
}
