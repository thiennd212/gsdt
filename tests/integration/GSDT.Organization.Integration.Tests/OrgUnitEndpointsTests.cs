using GSDT.Organization.Integration.Tests.Fixtures;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace GSDT.Organization.Integration.Tests;

/// <summary>
/// Integration tests for Organization (OrgUnit + Staff) REST API endpoints.
/// Uses WebApplicationFactory + Testcontainers SQL Server.
/// All write operations require Admin or SystemAdmin role.
/// Routes: /api/v1/admin/org/units, /api/v1/admin/org/staff
/// </summary>
[Collection(SqlServerCollection.CollectionName)]
public sealed class OrgUnitEndpointsTests : IClassFixture<WebAppFixture>
{
    private readonly WebAppFixture _app;

    // Fixed tenant IDs — deterministic across tests in the collection
    private static readonly Guid TenantA = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly Guid TenantB = Guid.Parse("20000000-0000-0000-0000-000000000002");
    private static readonly Guid AdminId = Guid.Parse("AD000000-0000-0000-0000-000000000001");
    private static readonly Guid StaffId = Guid.Parse("5F000000-0000-0000-0000-000000000001");

    public OrgUnitEndpointsTests(WebAppFixture app) => _app = app;

    // ── Helper ──────────────────────────────────────────────────────────────────

    /// <summary>Creates a root org unit and returns its ID. Fails the test if creation fails.</summary>
    private async Task<Guid> CreateOrgUnitAsync(
        HttpClient client,
        Guid tenantId,
        string name,
        string code,
        Guid? parentId = null)
    {
        var payload = new { name, nameEn = name + " EN", code, parentId };
        var resp = await client.PostAsJsonAsync(
            $"/api/v1/admin/org/units?tenantId={tenantId}", payload);
        resp.StatusCode.Should().Be(HttpStatusCode.OK,
            $"CreateOrgUnit '{name}' must succeed (status={resp.StatusCode})");
        var body = await resp.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("data").GetProperty("id").GetGuid();
    }

    // ── TC-ORG-INT-001: Create org unit via API ─────────────────────────────────

    [Fact]
    public async Task CreateOrgUnit_ValidPayload_Returns200AndCanBeRetrievedById()
    {
        var client = _app.CreateAuthenticatedClient(AdminId, TenantA, "Admin");
        var code = $"TC001-{Guid.NewGuid():N}"[..20]; // keep unique + within DB length

        var payload = new
        {
            name = "Ministry of Integration Tests",
            nameEn = "Ministry of Integration Tests EN",
            code,
            parentId = (Guid?)null
        };

        var createResp = await client.PostAsJsonAsync(
            $"/api/v1/admin/org/units?tenantId={TenantA}", payload);

        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var createBody = await createResp.Content.ReadFromJsonAsync<JsonElement>();
        var orgId = createBody.GetProperty("data").GetProperty("id").GetGuid();
        orgId.Should().NotBeEmpty();

        // GET by id
        var getResp = await client.GetAsync(
            $"/api/v1/admin/org/units/{orgId}?tenantId={TenantA}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var getData = (await getResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("data");
        getData.GetProperty("id").GetGuid().Should().Be(orgId);
        getData.GetProperty("code").GetString().Should().Be(code);
        getData.GetProperty("isActive").GetBoolean().Should().BeTrue();
    }

    // ── TC-ORG-INT-002: Child org unit level computed from parent ───────────────

    [Fact]
    public async Task CreateOrgUnit_WithParent_LevelIsParentLevelPlusOne()
    {
        var client = _app.CreateAuthenticatedClient(AdminId, TenantA, "Admin");

        // Create root (level 1)
        var rootId = await CreateOrgUnitAsync(client, TenantA,
            "TC002 Root", $"TC002-ROOT-{Guid.NewGuid():N}"[..20]);

        // Create child under root
        var childCode = $"TC002-CHILD-{Guid.NewGuid():N}"[..20];
        var childResp = await client.PostAsJsonAsync(
            $"/api/v1/admin/org/units?tenantId={TenantA}",
            new { name = "TC002 Child", nameEn = "TC002 Child EN", code = childCode, parentId = rootId });

        childResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var childData = (await childResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("data");

        childData.GetProperty("level").GetInt32().Should().Be(2,
            "child of a level-1 root must be level 2");
        childData.GetProperty("parentId").GetGuid().Should().Be(rootId);
    }

    // ── TC-ORG-INT-003: Duplicate code within tenant returns 409 ────────────────

    [Fact]
    public async Task CreateOrgUnit_DuplicateCodeSameTenant_Returns409Conflict()
    {
        var client = _app.CreateAuthenticatedClient(AdminId, TenantA, "Admin");
        var code = $"TC003-DUP-{Guid.NewGuid():N}"[..20];

        // First creation — must succeed
        await CreateOrgUnitAsync(client, TenantA, "TC003 Original", code);

        // Second creation with same code — must fail with 409
        var duplicateResp = await client.PostAsJsonAsync(
            $"/api/v1/admin/org/units?tenantId={TenantA}",
            new { name = "TC003 Duplicate", nameEn = "TC003 Duplicate EN", code, parentId = (Guid?)null });

        duplicateResp.StatusCode.Should().Be(HttpStatusCode.Conflict,
            "duplicate code within the same tenant must return 409");
    }

    // ── TC-ORG-INT-004: Deactivate with active children returns 422 ─────────────

    [Fact]
    public async Task DeleteOrgUnit_WithActiveChildren_Returns422()
    {
        var client = _app.CreateAuthenticatedClient(AdminId, TenantA, "Admin");

        // Create parent
        var parentId = await CreateOrgUnitAsync(client, TenantA,
            "TC004 Parent", $"TC004-PAR-{Guid.NewGuid():N}"[..20]);

        // Create child under parent
        await CreateOrgUnitAsync(client, TenantA,
            "TC004 Child", $"TC004-CHD-{Guid.NewGuid():N}"[..20], parentId);

        // Attempt to deactivate parent while child is still active
        var deleteResp = await client.DeleteAsync(
            $"/api/v1/admin/org/units/{parentId}?tenantId={TenantA}");

        deleteResp.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity,
            "deactivating a unit with active children must return 422");
    }

    // ── TC-ORG-INT-005: Get tree returns hierarchical structure ─────────────────

    [Fact]
    public async Task GetTree_AfterCreatingHierarchy_ReturnsAllUnitsForTenant()
    {
        var client = _app.CreateAuthenticatedClient(AdminId, TenantA, "Admin");

        // Build a small 2-level hierarchy unique to this test run
        var rootCode  = $"TC005-R-{Guid.NewGuid():N}"[..20];
        var childCode = $"TC005-C-{Guid.NewGuid():N}"[..20];

        var rootId = await CreateOrgUnitAsync(client, TenantA, "TC005 Root", rootCode);
        var childId = await CreateOrgUnitAsync(client, TenantA, "TC005 Child", childCode, rootId);

        // GET tree
        var treeResp = await client.GetAsync(
            $"/api/v1/admin/org/units?tenantId={TenantA}");
        treeResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var treeItems = (await treeResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("data")
            .EnumerateArray()
            .ToList();

        treeItems.Should().Contain(u => u.GetProperty("id").GetGuid() == rootId,
            "root unit must appear in tree");
        treeItems.Should().Contain(u => u.GetProperty("id").GetGuid() == childId,
            "child unit must appear in tree");

        var childNode = treeItems.First(u => u.GetProperty("id").GetGuid() == childId);
        childNode.GetProperty("parentId").GetGuid().Should().Be(rootId);
    }

    // ── TC-ORG-INT-006: Cross-tenant isolation ──────────────────────────────────

    [Fact]
    public async Task GetTree_TenantB_DoesNotSeeTenantAOrgUnits()
    {
        // Create an org unit under TenantA
        var clientA = _app.CreateAuthenticatedClient(AdminId, TenantA, "Admin");
        var code = $"TC006-A-{Guid.NewGuid():N}"[..20];
        var orgIdA = await CreateOrgUnitAsync(clientA, TenantA, "TC006 TenantA Unit", code);

        // Query tree as TenantB — must not see TenantA's unit
        var clientB = _app.CreateAuthenticatedClient(AdminId, TenantB, "Admin");
        var treeResp = await clientB.GetAsync(
            $"/api/v1/admin/org/units?tenantId={TenantB}");
        treeResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var treeItems = (await treeResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("data")
            .EnumerateArray()
            .ToList();

        treeItems.Should().NotContain(u => u.GetProperty("id").GetGuid() == orgIdA,
            "TenantB must not see org units belonging to TenantA");
    }

    // ── TC-ORG-INT-007: Assign staff creates assignment + history ────────────────

    [Fact]
    public async Task AssignStaff_ToActiveOrgUnit_CreatesAssignmentAndPositionHistory()
    {
        var client = _app.CreateAuthenticatedClient(AdminId, TenantA, "Admin", "SystemAdmin");

        // Create an org unit to assign to
        var unitId = await CreateOrgUnitAsync(client, TenantA,
            "TC007 Assignment Unit", $"TC007-U-{Guid.NewGuid():N}"[..20]);

        var staffId = Guid.NewGuid(); // unique staff per test run

        var assignPayload = new
        {
            orgUnitId = unitId,
            roleInOrg = "Officer",
            positionTitle = "Senior Integration Officer"
        };

        var assignResp = await client.PostAsJsonAsync(
            $"/api/v1/admin/org/staff/{staffId}/assign?tenantId={TenantA}", assignPayload);

        assignResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var assignData = (await assignResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("data");

        assignData.GetProperty("userId").GetGuid().Should().Be(staffId);
        assignData.GetProperty("orgUnitId").GetGuid().Should().Be(unitId);
        assignData.GetProperty("isActive").GetBoolean().Should().BeTrue();

        // Verify history is recorded
        var historyResp = await client.GetAsync(
            $"/api/v1/admin/org/staff/{staffId}/history?tenantId={TenantA}");
        historyResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var historyData = (await historyResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("data");

        historyData.GetProperty("assignments").EnumerateArray()
            .Should().Contain(a => a.GetProperty("orgUnitId").GetGuid() == unitId,
                "assignment must appear in staff history");

        historyData.GetProperty("positionHistory").EnumerateArray()
            .Should().Contain(p => p.GetProperty("positionTitle").GetString() == "Senior Integration Officer",
                "position history must be recorded on assignment");
    }

    // ── TC-ORG-INT-008: Transfer staff closes old, creates new ───────────────────

    [Fact]
    public async Task TransferStaff_FromOldUnit_ClosesOldAssignmentAndOpensNew()
    {
        var client = _app.CreateAuthenticatedClient(AdminId, TenantA, "Admin", "SystemAdmin");

        // Create two org units
        var unitFrom = await CreateOrgUnitAsync(client, TenantA,
            "TC008 From Unit", $"TC008-FROM-{Guid.NewGuid():N}"[..20]);
        var unitTo = await CreateOrgUnitAsync(client, TenantA,
            "TC008 To Unit", $"TC008-TO-{Guid.NewGuid():N}"[..20]);

        var staffId = Guid.NewGuid(); // unique per test run

        // Assign staff to 'from' unit first
        var assignPayload = new
        {
            orgUnitId = unitFrom,
            roleInOrg = "Officer",
            positionTitle = "Original Position"
        };
        var assignResp = await client.PostAsJsonAsync(
            $"/api/v1/admin/org/staff/{staffId}/assign?tenantId={TenantA}", assignPayload);
        assignResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // Transfer to 'to' unit
        var transferPayload = new
        {
            toOrgUnitId = unitTo,
            roleInOrg = "SeniorOfficer",
            positionTitle = "New Position After Transfer"
        };
        var transferResp = await client.PostAsJsonAsync(
            $"/api/v1/admin/org/staff/{staffId}/transfer?tenantId={TenantA}", transferPayload);

        transferResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var transferData = (await transferResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("data");

        // New assignment targets the 'to' unit and is active
        transferData.GetProperty("orgUnitId").GetGuid().Should().Be(unitTo);
        transferData.GetProperty("isActive").GetBoolean().Should().BeTrue();

        // Verify history: 2 assignments, old one closed
        var historyResp = await client.GetAsync(
            $"/api/v1/admin/org/staff/{staffId}/history?tenantId={TenantA}");
        historyResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var assignments = (await historyResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("data")
            .GetProperty("assignments")
            .EnumerateArray()
            .ToList();

        assignments.Should().HaveCount(2, "one original + one new after transfer");

        var closedAssignment = assignments.FirstOrDefault(
            a => a.GetProperty("orgUnitId").GetGuid() == unitFrom);
        closedAssignment.ValueKind.Should().NotBe(JsonValueKind.Undefined);
        closedAssignment.GetProperty("isActive").GetBoolean().Should().BeFalse(
            "old assignment must be closed (isActive=false) after transfer");

        var newAssignment = assignments.FirstOrDefault(
            a => a.GetProperty("orgUnitId").GetGuid() == unitTo);
        newAssignment.ValueKind.Should().NotBe(JsonValueKind.Undefined);
        newAssignment.GetProperty("isActive").GetBoolean().Should().BeTrue(
            "new assignment must be open (isActive=true) after transfer");
    }
}
