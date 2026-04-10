using System.Security.Claims;
using GSDT.Identity.Application.Authorization;
using GSDT.Identity.Domain.Models;
using GSDT.Identity.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using NSubstitute;

namespace GSDT.Security.Tests.Auth;

/// <summary>
/// Verifies that expired delegations do not grant permissions and active ones do.
/// TC-SEC-DELEG-001 to 002 — simulated via IEffectivePermissionService mock.
/// EffectivePermissionService already filters out expired delegations before building
/// the summary; these tests confirm the authorization layer respects the result.
/// </summary>
public sealed class DelegationExpiryTests
{
    private static ClaimsPrincipal CreateUser(Guid userId)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }

    private static AuthorizationHandlerContext CreateContext(
        ClaimsPrincipal user, string requiredPermission)
    {
        var requirement = new PermissionRequirement(requiredPermission);
        return new AuthorizationHandlerContext([requirement], user, null);
    }

    [Fact]
    [Trait("Category", "Security")]
    public async Task ExpiredDelegation_DoesNotGrantPermission()
    {
        // When delegation is expired, EffectivePermissionService returns empty set.
        // Authorization handler must deny access — no stale-cache fallback allowed.
        var userId = Guid.NewGuid();
        var permService = Substitute.For<IEffectivePermissionService>();
        permService.GetSummaryAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new EffectivePermissionSummary
            {
                UserId = userId,
                PermissionCodes = new HashSet<string>() // expired delegation → empty
            });

        var handler = new PermissionAuthorizationHandler(permService);
        var user = CreateUser(userId);
        var ctx = CreateContext(user, Permissions.Inv.DomesticWrite);

        await handler.HandleAsync(ctx);

        ctx.HasSucceeded.Should().BeFalse(
            "expired delegation must not grant any permissions");
    }

    [Fact]
    [Trait("Category", "Security")]
    public async Task ActiveDelegation_GrantsPermission()
    {
        // When delegation is active, EffectivePermissionService includes delegated codes.
        // Authorization handler must succeed for a delegated permission.
        var userId = Guid.NewGuid();
        var permService = Substitute.For<IEffectivePermissionService>();
        permService.GetSummaryAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new EffectivePermissionSummary
            {
                UserId = userId,
                // Active delegation grants DomesticWrite to this user temporarily
                PermissionCodes = new HashSet<string> { Permissions.Inv.DomesticWrite }
            });

        var handler = new PermissionAuthorizationHandler(permService);
        var user = CreateUser(userId);
        var ctx = CreateContext(user, Permissions.Inv.DomesticWrite);

        await handler.HandleAsync(ctx);

        ctx.HasSucceeded.Should().BeTrue(
            "active delegation must grant the delegated permission");
    }
}
