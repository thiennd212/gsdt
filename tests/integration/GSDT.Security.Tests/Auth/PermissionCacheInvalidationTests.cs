using System.Security.Claims;
using GSDT.Identity.Application.Authorization;
using GSDT.Identity.Domain.Models;
using GSDT.Identity.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using NSubstitute;

namespace GSDT.Security.Tests.Auth;

/// <summary>
/// Simulates permission cache invalidation scenarios using NSubstitute sequential returns.
/// Verifies that revocation takes effect on next call and grant becomes visible on next call.
/// TC-SEC-CACHE-001 to 002.
/// </summary>
public sealed class PermissionCacheInvalidationTests
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
    public async Task AfterPermissionRevocation_AccessIsDenied()
    {
        // First call: permission present (cache hit from before revocation).
        // Second call: permission absent (cache invalidated after revocation).
        // Both authorization decisions must reflect what the service returns.
        var userId = Guid.NewGuid();
        var permService = Substitute.For<IEffectivePermissionService>();

        var withPermission = new EffectivePermissionSummary
        {
            UserId = userId,
            PermissionCodes = new HashSet<string> { Permissions.Inv.DomesticWrite }
        };
        var withoutPermission = new EffectivePermissionSummary
        {
            UserId = userId,
            PermissionCodes = new HashSet<string>() // revoked
        };

        // Sequential: first call returns permission, second call returns empty (post-invalidation)
        permService.GetSummaryAsync(userId, Arg.Any<CancellationToken>())
            .Returns(withPermission, withoutPermission);

        var handler = new PermissionAuthorizationHandler(permService);
        var user = CreateUser(userId);

        // First request — permission is present
        var ctx1 = CreateContext(user, Permissions.Inv.DomesticWrite);
        await handler.HandleAsync(ctx1);
        ctx1.HasSucceeded.Should().BeTrue("permission was present before revocation");

        // Second request — permission was revoked (cache invalidated)
        var ctx2 = CreateContext(user, Permissions.Inv.DomesticWrite);
        await handler.HandleAsync(ctx2);
        ctx2.HasSucceeded.Should().BeFalse("access must be denied after permission revocation");
    }

    [Fact]
    [Trait("Category", "Security")]
    public async Task AfterPermissionGrant_AccessIsAllowed()
    {
        // First call: permission absent (cache hit from before grant).
        // Second call: permission present (cache invalidated after grant).
        var userId = Guid.NewGuid();
        var permService = Substitute.For<IEffectivePermissionService>();

        var withoutPermission = new EffectivePermissionSummary
        {
            UserId = userId,
            PermissionCodes = new HashSet<string>() // not yet granted
        };
        var withPermission = new EffectivePermissionSummary
        {
            UserId = userId,
            PermissionCodes = new HashSet<string> { Permissions.Inv.OdaWrite } // newly granted
        };

        // Sequential: first call empty, second call has permission (post-grant invalidation)
        permService.GetSummaryAsync(userId, Arg.Any<CancellationToken>())
            .Returns(withoutPermission, withPermission);

        var handler = new PermissionAuthorizationHandler(permService);
        var user = CreateUser(userId);

        // First request — permission not yet granted
        var ctx1 = CreateContext(user, Permissions.Inv.OdaWrite);
        await handler.HandleAsync(ctx1);
        ctx1.HasSucceeded.Should().BeFalse("permission was not yet granted");

        // Second request — permission granted and cache refreshed
        var ctx2 = CreateContext(user, Permissions.Inv.OdaWrite);
        await handler.HandleAsync(ctx2);
        ctx2.HasSucceeded.Should().BeTrue("access must be allowed after permission grant");
    }
}
