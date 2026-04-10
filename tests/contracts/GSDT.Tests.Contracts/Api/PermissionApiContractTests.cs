using System.Text.Json;
using GSDT.Identity.Application.Authorization;
using GSDT.Identity.Application.Queries.GetPermissions;
using GSDT.Identity.Application.Queries.GetRoleById;
using GSDT.SharedKernel.Api;

namespace GSDT.Tests.Contracts.Api;

/// <summary>
/// Contract tests for Permission and Role DTOs — TC-CON-PERM-001 through TC-CON-PERM-009.
/// Covers PermissionDto, RoleDetailDto, RolePermissionDto, ApiError shape,
/// and PermissionSeedDefinitions structural invariants.
/// Pure serialization tests — no DB, no HTTP.
/// </summary>
public sealed class PermissionApiContractTests
{
    // ── PermissionDto ─────────────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Contract")]
    public void PermissionDto_JsonShape_RequiredFieldsPresent()
    {
        var dto = new PermissionDto(
            Guid.NewGuid(),
            "INV.DOMESTIC.READ",
            "Xem du an trong nuoc",
            "Permission to read domestic projects",
            "INV",
            "DOMESTIC",
            "READ");

        var json = JsonSerializer.Serialize(dto);
        var root = JsonDocument.Parse(json).RootElement;

        root.TryGetProperty("Id", out _).Should().BeTrue();
        root.TryGetProperty("Code", out _).Should().BeTrue();
        root.TryGetProperty("Name", out _).Should().BeTrue();
        root.TryGetProperty("ModuleCode", out _).Should().BeTrue();
        root.TryGetProperty("ResourceCode", out _).Should().BeTrue();
        root.TryGetProperty("ActionCode", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Contract")]
    public void PermissionDto_RoundTrip_Lossless()
    {
        var dto = new PermissionDto(
            Guid.NewGuid(),
            "ADMIN.PERM.ASSIGN",
            "Gan quyen cho vai tro",
            "Sensitive permission to assign roles",
            "ADMIN",
            "PERM",
            "ASSIGN");

        var json = JsonSerializer.Serialize(dto);
        var d = JsonSerializer.Deserialize<PermissionDto>(json);

        d.Should().NotBeNull();
        d!.Id.Should().Be(dto.Id);
        d.Code.Should().Be(dto.Code);
        d.Name.Should().Be(dto.Name);
        d.ModuleCode.Should().Be(dto.ModuleCode);
        d.ResourceCode.Should().Be(dto.ResourceCode);
        d.ActionCode.Should().Be(dto.ActionCode);
    }

    [Theory]
    [Trait("Category", "Contract")]
    [InlineData("INV.DOMESTIC.READ")]
    [InlineData("INV.ODA.WRITE")]
    [InlineData("ADMIN.ROLE.DELETE")]
    [InlineData("ADMIN.PERM.ASSIGN")]
    public void PermissionCode_Format_FollowsModuleDotResourceDotAction(string code)
    {
        var parts = code.Split('.');
        parts.Should().HaveCount(3, because: "permission code must follow MODULE.RESOURCE.ACTION format");
        parts[0].Should().NotBeNullOrWhiteSpace("MODULE segment must not be empty");
        parts[1].Should().NotBeNullOrWhiteSpace("RESOURCE segment must not be empty");
        parts[2].Should().NotBeNullOrWhiteSpace("ACTION segment must not be empty");
    }

    // ── RoleDetailDto ─────────────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Contract")]
    public void RoleDetailDto_JsonShape_RequiredFieldsPresent()
    {
        var dto = CreateSampleRoleDetailDto();
        var json = JsonSerializer.Serialize(dto);
        var root = JsonDocument.Parse(json).RootElement;

        root.TryGetProperty("Id", out _).Should().BeTrue();
        root.TryGetProperty("Code", out _).Should().BeTrue();
        root.TryGetProperty("Name", out _).Should().BeTrue();
        root.TryGetProperty("RoleType", out _).Should().BeTrue();
        root.TryGetProperty("IsActive", out _).Should().BeTrue();
        root.TryGetProperty("Permissions", out var perms).Should().BeTrue();
        perms.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    [Trait("Category", "Contract")]
    public void RoleDetailDto_RoundTrip_Lossless()
    {
        var dto = CreateSampleRoleDetailDto();
        var json = JsonSerializer.Serialize(dto);
        var d = JsonSerializer.Deserialize<RoleDetailDto>(json);

        d.Should().NotBeNull();
        d!.Id.Should().Be(dto.Id);
        d.Code.Should().Be(dto.Code);
        d.Name.Should().Be(dto.Name);
        d.RoleType.Should().Be(dto.RoleType);
        d.IsActive.Should().Be(dto.IsActive);
        d.Permissions.Should().HaveCount(2);
    }

    // ── RolePermissionDto ─────────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Contract")]
    public void RolePermissionDto_JsonShape_RequiredFieldsPresent()
    {
        var dto = new RolePermissionDto(
            Guid.NewGuid(),
            "INV.DOMESTIC.READ",
            "Xem du an trong nuoc");

        var json = JsonSerializer.Serialize(dto);
        var root = JsonDocument.Parse(json).RootElement;

        root.TryGetProperty("PermissionId", out _).Should().BeTrue();
        root.TryGetProperty("Code", out _).Should().BeTrue();
        root.TryGetProperty("Name", out _).Should().BeTrue();
    }

    // ── ApiError shape ────────────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Contract")]
    public void ApiError_JsonShape_HasGovPrefixCodeAndRequiredFields()
    {
        var error = new ApiError("GOV_INV_001", "Project not found", "Khong tim thay du an", "trace-abc-123");
        var json = JsonSerializer.Serialize(error);
        var root = JsonDocument.Parse(json).RootElement;

        root.TryGetProperty("Code", out var code).Should().BeTrue();
        code.GetString().Should().StartWith("GOV_", because: "error codes must follow GOV_MODULE_NNN convention");

        root.TryGetProperty("Message", out _).Should().BeTrue();
        root.TryGetProperty("DetailVi", out _).Should().BeTrue();
        root.TryGetProperty("TraceId", out _).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Contract")]
    public void ApiResponse_Fail_Shape_SuccessIsFalseAndErrorsArrayNotEmpty()
    {
        var errors = new List<FluentResults.Error> { new("Permission denied") };
        var response = ApiResponse<object>.Fail(errors);
        var json = JsonSerializer.Serialize(response);
        var root = JsonDocument.Parse(json).RootElement;

        root.TryGetProperty("Success", out var success).Should().BeTrue();
        success.GetBoolean().Should().BeFalse();

        root.TryGetProperty("Errors", out var errArr).Should().BeTrue();
        errArr.ValueKind.Should().Be(JsonValueKind.Array);
        errArr.GetArrayLength().Should().BeGreaterThan(0);
    }

    // ── PermissionSeedDefinitions structural invariants ───────────────────────

    [Fact]
    [Trait("Category", "Contract")]
    public void PermissionSeedDefinitions_AllPermissions_Has23PermissionsNoDuplicates()
    {
        var all = PermissionSeedDefinitions.AllPermissions;

        all.Should().HaveCount(23, because: "seed matrix defines exactly 18 investment + 5 admin permissions");

        var codes = all.Select(p => p.Code).ToList();
        codes.Should().OnlyHaveUniqueItems(because: "permission codes must be unique — duplicates break RBAC seeding");
    }

    [Fact]
    [Trait("Category", "Contract")]
    public void PermissionSeedDefinitions_RolePermissionMap_CorrectRoleCounts()
    {
        var map = PermissionSeedDefinitions.RolePermissionMap;

        // BTC: all 18 investment permissions
        map["BTC"].Should().HaveCount(18, because: "BTC has full investment access (6 types × 3 actions)");

        // CDT: all 18 investment permissions
        map["CDT"].Should().HaveCount(18, because: "CDT has full investment access (6 types × 3 actions)");

        // CQCQ: READ-only — 6 project types × 1 action
        map["CQCQ"].Should().HaveCount(6, because: "CQCQ has read-only access (6 types × 1 READ action)");

        // Admin: all 23 permissions
        map["Admin"].Should().HaveCount(23, because: "Admin has all investment + all admin permissions");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static RoleDetailDto CreateSampleRoleDetailDto() =>
        new(
            Id: Guid.NewGuid(),
            Code: "BTC",
            Name: "Bo Tai Chinh",
            Description: "Bo Tai Chinh role with full investment access",
            RoleType: "System",
            IsActive: true,
            Permissions:
            [
                new RolePermissionDto(Guid.NewGuid(), "INV.DOMESTIC.READ", "Xem du an trong nuoc"),
                new RolePermissionDto(Guid.NewGuid(), "INV.DOMESTIC.WRITE", "Tao/sua du an trong nuoc")
            ]);
}
