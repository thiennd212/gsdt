using System.Text.RegularExpressions;
using GSDT.Identity.Application.Authorization;

namespace GSDT.Security.Tests.Auth;

/// <summary>
/// Verifies the integrity of PermissionSeedDefinitions — role-permission counts and
/// permission code format. Protects against accidental edits that silently break RBAC.
/// TC-SEC-SEED-001 to 002.
/// </summary>
public sealed partial class SystemRoleProtectionTests
{
    // Regex: MODULE.RESOURCE.ACTION — each segment is 2–20 uppercase letters/digits
    [GeneratedRegex(@"^[A-Z][A-Z0-9]{1,19}\.[A-Z][A-Z0-9]{1,19}\.[A-Z][A-Z0-9]{1,19}$")]
    private static partial Regex PermissionCodeRegex();

    [Fact]
    [Trait("Category", "Security")]
    public void PermissionSeedDefinitions_HasCorrectRolePermissionCounts()
    {
        // BTC  = 18 (all 6 project types × 3 actions: READ/WRITE/DELETE)
        // CQCQ =  6 (all 6 project types × READ only)
        // CDT  = 18 (all 6 project types × 3 actions: READ/WRITE/DELETE)
        // Admin = 23 (18 investment + 5 admin: ROLE.READ/WRITE/DELETE, PERM.READ, PERM.ASSIGN)
        var map = PermissionSeedDefinitions.RolePermissionMap;

        map["BTC"].Should().HaveCount(18,
            "BTC has all 6 project types × 3 actions");
        map["CQCQ"].Should().HaveCount(6,
            "CQCQ has READ-only across 6 project types");
        map["CDT"].Should().HaveCount(18,
            "CDT has all 6 project types × 3 actions");
        map["Admin"].Should().HaveCount(23,
            "Admin has 18 investment + 5 admin permissions");
        map["SystemAdmin"].Should().HaveCount(23,
            "SystemAdmin has same permissions as Admin");
    }

    [Fact]
    [Trait("Category", "Security")]
    public void AllPermissionCodes_FollowModuleResourceActionFormat()
    {
        // Every code in AllPermissions must match MODULE.RESOURCE.ACTION regex.
        // Catches typos or format deviations added during future feature work.
        var invalidCodes = PermissionSeedDefinitions.AllPermissions
            .Select(p => p.Code)
            .Where(code => !PermissionCodeRegex().IsMatch(code))
            .ToList();

        invalidCodes.Should().BeEmpty(
            $"all permission codes must match MODULE.RESOURCE.ACTION format; " +
            $"invalid: {string.Join(", ", invalidCodes)}");
    }
}
