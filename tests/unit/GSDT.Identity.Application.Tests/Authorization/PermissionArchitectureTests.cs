using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using GSDT.Identity.Application.Authorization;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace GSDT.Identity.Application.Tests.Authorization;

/// <summary>
/// Architecture tests for permission-based authorization.
/// Reflection-based — no DB or HTTP stack required.
/// Validates namespace placement, interface contracts, and constant naming conventions.
/// </summary>
public sealed class PermissionArchitectureTests
{
    private const string AuthorizationNamespace = "GSDT.Identity.Application.Authorization";
    private static readonly Regex PermissionCodeRegex = new(@"^[A-Z]+\.[A-Z]+\.[A-Z]+$", RegexOptions.Compiled);

    // ── 1. PermissionPolicyProvider namespace ────────────────────────────────

    [Fact]
    public void PermissionPolicyProvider_IsInCorrectNamespace()
    {
        typeof(PermissionPolicyProvider).Namespace
            .Should().Be(AuthorizationNamespace);
    }

    // ── 2. RequirePermissionAttribute namespace ──────────────────────────────

    [Fact]
    public void RequirePermissionAttribute_IsInCorrectNamespace()
    {
        typeof(RequirePermissionAttribute).Namespace
            .Should().Be(AuthorizationNamespace);
    }

    // ── 3. PermissionConstants follow MODULE.RESOURCE.ACTION format ──────────

    [Fact]
    public void AllPermissionConstants_MatchModuleResourceActionFormat()
    {
        var constants = GetAllPermissionConstantValues();
        constants.Should().NotBeEmpty("Permissions class must declare at least one constant");

        foreach (var code in constants)
        {
            PermissionCodeRegex.IsMatch(code).Should().BeTrue(
                $"permission constant '{code}' must match MODULE.RESOURCE.ACTION format");
        }
    }

    // ── 4. Seed codes are all defined as constants ───────────────────────────

    [Fact]
    public void AllSeedPermissionCodes_AreDefinedAsConstants()
    {
        var constantCodes = GetAllPermissionConstantValues().ToHashSet();
        var seedCodes = PermissionSeedDefinitions.AllPermissions
            .Select(p => p.Code)
            .ToList();

        seedCodes.Should().NotBeEmpty();
        foreach (var code in seedCodes)
        {
            constantCodes.Should().Contain(code,
                $"seed code '{code}' must have a matching constant in Permissions class");
        }
    }

    // ── 5. PermissionPolicyProvider implements IAuthorizationPolicyProvider ──

    [Fact]
    public void PermissionPolicyProvider_ImplementsIAuthorizationPolicyProvider()
    {
        typeof(PermissionPolicyProvider)
            .GetInterfaces()
            .Should().Contain(typeof(IAuthorizationPolicyProvider),
                "PermissionPolicyProvider must implement IAuthorizationPolicyProvider");
    }

    // ── 6. RequirePermissionAttribute extends AuthorizeAttribute ─────────────

    [Fact]
    public void RequirePermissionAttribute_ExtendsAuthorizeAttribute()
    {
        typeof(RequirePermissionAttribute)
            .IsSubclassOf(typeof(AuthorizeAttribute))
            .Should().BeTrue(
                "RequirePermissionAttribute must extend AuthorizeAttribute");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static IEnumerable<string> GetAllPermissionConstantValues()
        => typeof(Permissions)
            .GetNestedTypes(BindingFlags.Public | BindingFlags.Static)
            .SelectMany(t => t.GetFields(
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!);
}
