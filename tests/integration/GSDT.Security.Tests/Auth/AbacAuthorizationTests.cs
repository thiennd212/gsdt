using System.Security.Claims;
using GSDT.Identity.Application.Authorization;
using GSDT.Identity.Domain.Entities;
using GSDT.Identity.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;

namespace GSDT.Security.Tests.Auth;

/// <summary>
/// ABAC authorization tests — TC-SEC-AUTHZ-004, TC-SEC-AUTHZ-005.
/// Tests department-based ABAC policy with admin bypass and deny-wins-over-allow.
/// </summary>
public sealed class AbacAuthorizationTests
{
    private static ClaimsPrincipal CreateUser(string? department = null, params string[] roles)
    {
        var claims = new List<Claim>();
        if (department != null)
            claims.Add(new Claim("department", department));
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }

    private static AuthorizationHandlerContext CreateContext(ClaimsPrincipal user)
    {
        var requirement = new DepartmentAccessRequirement();
        return new AuthorizationHandlerContext([requirement], user, null);
    }

    [Fact]
    [Trait("Category", "Security")]
    public async Task AdminRole_BypassesAbacChecks()
    {
        var rules = Substitute.For<IAttributeRuleRepository>();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new AbacAuthorizationHandler(rules, cache);

        var user = CreateUser("CNTT", Roles.Admin);
        var ctx = CreateContext(user);

        await handler.HandleAsync(ctx);

        ctx.HasSucceeded.Should().BeTrue();
        // DB should never be hit for admin
        await rules.DidNotReceive().GetByAttributeAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    [Trait("Category", "Security")]
    public async Task SystemAdminRole_BypassesAbacChecks()
    {
        var rules = Substitute.For<IAttributeRuleRepository>();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new AbacAuthorizationHandler(rules, cache);

        var user = CreateUser(null, Roles.SystemAdmin);
        var ctx = CreateContext(user);

        await handler.HandleAsync(ctx);

        ctx.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Security")]
    public async Task NoDepartmentClaim_DeniedImplicitly()
    {
        var rules = Substitute.For<IAttributeRuleRepository>();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new AbacAuthorizationHandler(rules, cache);

        var user = CreateUser(department: null); // no department
        var ctx = CreateContext(user);

        await handler.HandleAsync(ctx);

        ctx.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Security")]
    public async Task AllowRule_GrantsAccess()
    {
        var rules = Substitute.For<IAttributeRuleRepository>();
        rules.GetByAttributeAsync("department", "CNTT", Arg.Any<CancellationToken>())
            .Returns([new AttributeRule { Effect = AttributeEffect.Allow }]);

        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new AbacAuthorizationHandler(rules, cache);

        var user = CreateUser("CNTT");
        var ctx = CreateContext(user);

        await handler.HandleAsync(ctx);

        ctx.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Security")]
    public async Task DenyRule_OverridesAllowRule()
    {
        var rules = Substitute.For<IAttributeRuleRepository>();
        rules.GetByAttributeAsync("department", "RESTRICTED", Arg.Any<CancellationToken>())
            .Returns([
                new AttributeRule { Effect = AttributeEffect.Allow },
                new AttributeRule { Effect = AttributeEffect.Deny }
            ]);

        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new AbacAuthorizationHandler(rules, cache);

        var user = CreateUser("RESTRICTED");
        var ctx = CreateContext(user);

        await handler.HandleAsync(ctx);

        ctx.HasSucceeded.Should().BeFalse("Deny must override Allow");
    }

    [Fact]
    [Trait("Category", "Security")]
    public async Task NoRules_FallbackToDepartmentCodeClaim()
    {
        var rules = Substitute.For<IAttributeRuleRepository>();
        rules.GetByAttributeAsync("department", "TCCB", Arg.Any<CancellationToken>())
            .Returns(new List<AttributeRule>());

        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new AbacAuthorizationHandler(rules, cache);

        // User has matching department_code claim
        var claims = new List<Claim>
        {
            new("department", "TCCB"),
            new("department_code", "TCCB")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var ctx = CreateContext(user);

        await handler.HandleAsync(ctx);

        ctx.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Security")]
    public async Task NoRules_MismatchedDeptCode_Denied()
    {
        var rules = Substitute.For<IAttributeRuleRepository>();
        rules.GetByAttributeAsync("department", "CNTT", Arg.Any<CancellationToken>())
            .Returns(new List<AttributeRule>());

        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new AbacAuthorizationHandler(rules, cache);

        var claims = new List<Claim>
        {
            new("department", "CNTT"),
            new("department_code", "TCCB") // mismatch
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var ctx = CreateContext(user);

        await handler.HandleAsync(ctx);

        ctx.HasSucceeded.Should().BeFalse();
    }
}
