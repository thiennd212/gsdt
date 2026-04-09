using FluentAssertions;
using GSDT.Identity.Application.Authorization;
using Xunit;

namespace GSDT.Identity.Application.Tests.Authorization;

/// <summary>
/// Unit tests for PermissionSeedDefinitions.
/// Validates seed data completeness, role-permission counts, code matching with constants.
/// </summary>
public sealed class PermissionSeedDefinitionsTests
{
    [Fact]
    public void AllPermissions_Contains23Entries()
    {
        PermissionSeedDefinitions.AllPermissions.Should().HaveCount(23);
    }

    [Fact]
    public void AllPermissions_HasNoDuplicateCodes()
    {
        var codes = PermissionSeedDefinitions.AllPermissions.Select(p => p.Code).ToList();
        codes.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void AllPermissions_AllFieldsPopulated()
    {
        foreach (var perm in PermissionSeedDefinitions.AllPermissions)
        {
            perm.Code.Should().NotBeNullOrWhiteSpace($"Code for {perm.Name}");
            perm.Name.Should().NotBeNullOrWhiteSpace($"Name for {perm.Code}");
            perm.ModuleCode.Should().NotBeNullOrWhiteSpace($"ModuleCode for {perm.Code}");
            perm.ResourceCode.Should().NotBeNullOrWhiteSpace($"ResourceCode for {perm.Code}");
            perm.ActionCode.Should().NotBeNullOrWhiteSpace($"ActionCode for {perm.Code}");
        }
    }

    [Fact]
    public void PermissionCodes_MatchConstants()
    {
        // Every seed code must exist as a constant in Permissions class
        var seedCodes = PermissionSeedDefinitions.AllPermissions
            .Select(p => p.Code)
            .ToHashSet();

        // Collect all constants from Permissions nested classes via reflection
        var constantCodes = typeof(Permissions)
            .GetNestedTypes()
            .SelectMany(t => t.GetFields(
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Static |
                System.Reflection.BindingFlags.FlattenHierarchy))
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!)
            .ToHashSet();

        // All seed codes must be defined as constants
        seedCodes.Should().BeSubsetOf(constantCodes,
            "every seed permission code must have a matching constant in Permissions class");
    }

    [Theory]
    [InlineData("BTC", 18)]
    [InlineData("CQCQ", 6)]
    [InlineData("CDT", 18)]
    [InlineData("Admin", 23)]
    public void RolePermissionMap_HasCorrectCounts(string role, int expectedCount)
    {
        PermissionSeedDefinitions.RolePermissionMap.Should().ContainKey(role);
        PermissionSeedDefinitions.RolePermissionMap[role].Should().HaveCount(expectedCount);
    }

    [Fact]
    public void RolePermissionMap_CqcqOnlyHasReadPermissions()
    {
        var cqcqCodes = PermissionSeedDefinitions.RolePermissionMap["CQCQ"];
        cqcqCodes.Should().AllSatisfy(code => code.Should().EndWith(".READ"));
    }

    [Fact]
    public void RolePermissionMap_AllCodesExistInAllPermissions()
    {
        var allCodes = PermissionSeedDefinitions.AllPermissions
            .Select(p => p.Code)
            .ToHashSet();

        foreach (var (role, codes) in PermissionSeedDefinitions.RolePermissionMap)
        {
            codes.Should().AllSatisfy(code =>
                allCodes.Should().Contain(code,
                    $"role '{role}' references code '{code}' not in AllPermissions"));
        }
    }
}
