using System.Security.Claims;
using GSDT.Identity.Application.Authorization;
using GSDT.Identity.Domain.Models;
using GSDT.Identity.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using NSubstitute;

namespace GSDT.Security.Tests.Auth;

/// <summary>
/// Verifies that roles cannot escalate beyond their assigned permission set.
/// TC-SEC-ESC-001 to 004 — uses NSubstitute mocks for EffectivePermissionService.
/// </summary>
public sealed class PermissionEscalationTests
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
    public async Task CqcqUser_CannotEscalateTo_WritePermission()
    {
        // CQCQ has only READ permissions — must not gain WRITE via escalation
        var userId = Guid.NewGuid();
        var permService = Substitute.For<IEffectivePermissionService>();
        permService.GetSummaryAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new EffectivePermissionSummary
            {
                UserId = userId,
                PermissionCodes = new HashSet<string>
                {
                    Permissions.Inv.DomesticRead,
                    Permissions.Inv.OdaRead,
                    Permissions.Inv.PppRead,
                    Permissions.Inv.DnnnRead,
                    Permissions.Inv.NdtRead,
                    Permissions.Inv.FdiRead,
                }
            });

        var handler = new PermissionAuthorizationHandler(permService);
        var user = CreateUser(userId);
        var ctx = CreateContext(user, Permissions.Inv.DomesticWrite);

        await handler.HandleAsync(ctx);

        ctx.HasSucceeded.Should().BeFalse("CQCQ must not have WRITE permission");
    }

    [Fact]
    [Trait("Category", "Security")]
    public async Task CqcqUser_CannotEscalateTo_DeletePermission()
    {
        // CQCQ has only READ permissions — must not gain DELETE via escalation
        var userId = Guid.NewGuid();
        var permService = Substitute.For<IEffectivePermissionService>();
        permService.GetSummaryAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new EffectivePermissionSummary
            {
                UserId = userId,
                PermissionCodes = new HashSet<string>
                {
                    Permissions.Inv.DomesticRead,
                    Permissions.Inv.OdaRead,
                    Permissions.Inv.PppRead,
                    Permissions.Inv.DnnnRead,
                    Permissions.Inv.NdtRead,
                    Permissions.Inv.FdiRead,
                }
            });

        var handler = new PermissionAuthorizationHandler(permService);
        var user = CreateUser(userId);
        var ctx = CreateContext(user, Permissions.Inv.FdiDelete);

        await handler.HandleAsync(ctx);

        ctx.HasSucceeded.Should().BeFalse("CQCQ must not have DELETE permission");
    }

    [Fact]
    [Trait("Category", "Security")]
    public async Task BtcUser_CannotAccess_AdminPermissions()
    {
        // BTC has 18 investment permissions only — must not access ADMIN.ROLE.DELETE
        var userId = Guid.NewGuid();
        var permService = Substitute.For<IEffectivePermissionService>();

        // BTC has all investment permissions but none of the admin permissions
        var btcPermissions = PermissionSeedDefinitions.RolePermissionMap["BTC"];
        permService.GetSummaryAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new EffectivePermissionSummary
            {
                UserId = userId,
                PermissionCodes = new HashSet<string>(btcPermissions)
            });

        var handler = new PermissionAuthorizationHandler(permService);
        var user = CreateUser(userId);
        var ctx = CreateContext(user, Permissions.Admin.RoleDelete);

        await handler.HandleAsync(ctx);

        ctx.HasSucceeded.Should().BeFalse("BTC must not have ADMIN.ROLE.DELETE permission");
    }

    [Fact]
    [Trait("Category", "Security")]
    public async Task EmptyPermissionSet_DeniesAllRequests()
    {
        // No permissions at all — every requirement must be denied
        var userId = Guid.NewGuid();
        var permService = Substitute.For<IEffectivePermissionService>();
        permService.GetSummaryAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new EffectivePermissionSummary
            {
                UserId = userId,
                PermissionCodes = new HashSet<string>()
            });

        var handler = new PermissionAuthorizationHandler(permService);
        var user = CreateUser(userId);
        var ctx = CreateContext(user, Permissions.Inv.DomesticRead);

        await handler.HandleAsync(ctx);

        ctx.HasSucceeded.Should().BeFalse("Empty permission set must deny all access");
    }
}
