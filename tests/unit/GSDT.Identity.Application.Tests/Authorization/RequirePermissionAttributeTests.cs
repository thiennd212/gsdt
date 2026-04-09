using FluentAssertions;
using GSDT.Identity.Application.Authorization;
using Xunit;

namespace GSDT.Identity.Application.Tests.Authorization;

/// <summary>
/// Unit tests for RequirePermissionAttribute.
/// Verifies: policy name format, case preservation.
/// </summary>
public sealed class RequirePermissionAttributeTests
{
    [Fact]
    public void Constructor_SetsPolicy_WithPermPrefix()
    {
        var attr = new RequirePermissionAttribute("INV.DOMESTIC.READ");

        attr.Policy.Should().Be("PERM:INV.DOMESTIC.READ");
    }

    [Fact]
    public void Constructor_PreservesExactCode_NoCaseTransform()
    {
        var attr = new RequirePermissionAttribute("admin.role.read");

        attr.Policy.Should().Be("PERM:admin.role.read");
    }
}
