using FluentAssertions;
using GSDT.Identity.Application.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Xunit;

namespace GSDT.Identity.Application.Tests.Authorization;

/// <summary>
/// Unit tests for PermissionPolicyProvider.
/// Verifies: PERM: prefix routing, fallback delegation, deny policy for empty codes, caching.
/// </summary>
public sealed class PermissionPolicyProviderTests
{
    private readonly PermissionPolicyProvider _sut;

    public PermissionPolicyProviderTests()
    {
        var options = Options.Create(new AuthorizationOptions());
        _sut = new PermissionPolicyProvider(options);
    }

    [Fact]
    public async Task GetPolicyAsync_WithPermPrefix_ReturnsPolicy_ContainingPermissionRequirement()
    {
        // Act
        var policy = await _sut.GetPolicyAsync("PERM:INV.DOMESTIC.READ");

        // Assert
        policy.Should().NotBeNull();
        policy!.Requirements.Should().HaveCount(2); // RequireAuthenticatedUser + PermissionRequirement
        policy.Requirements.OfType<PermissionRequirement>().Should().ContainSingle()
            .Which.PermissionCode.Should().Be("INV.DOMESTIC.READ");
    }

    [Fact]
    public async Task GetPolicyAsync_WithoutPermPrefix_DelegatesToFallback()
    {
        // Act — "Admin" is not registered, fallback returns null
        var policy = await _sut.GetPolicyAsync("Admin");

        // Assert — DefaultAuthorizationPolicyProvider returns null for unregistered policies
        policy.Should().BeNull();
    }

    [Fact]
    public async Task GetPolicyAsync_EmptyCodeAfterPrefix_ReturnsDenyPolicy()
    {
        // C3 fix: empty code must NOT return null (would cause InvalidOperationException).
        // Instead returns a deny policy that always rejects.
        var policy = await _sut.GetPolicyAsync("PERM:");

        // Assert — policy exists but has an unsatisfiable claim requirement
        policy.Should().NotBeNull();
        // The deny policy should require authentication + impossible claim
        policy!.Requirements.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetDefaultPolicyAsync_DelegatesToFallback()
    {
        // Act
        var policy = await _sut.GetDefaultPolicyAsync();

        // Assert — default policy requires authenticated user
        policy.Should().NotBeNull();
    }

    [Fact]
    public async Task GetFallbackPolicyAsync_DelegatesToFallback()
    {
        // Act
        var policy = await _sut.GetFallbackPolicyAsync();

        // Assert — fallback is null by default (no anonymous access policy)
        policy.Should().BeNull();
    }

    [Fact]
    public async Task GetPolicyAsync_SameCode_ReturnsCachedInstance()
    {
        // H7: policies should be cached in ConcurrentDictionary
        var first = await _sut.GetPolicyAsync("PERM:INV.ODA.READ");
        var second = await _sut.GetPolicyAsync("PERM:INV.ODA.READ");

        // Assert — same reference = cached
        first.Should().BeSameAs(second);
    }
}
