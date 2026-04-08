using FluentAssertions;
using NetArchTest.Rules;
using GSDT.Architecture.Tests.Fixtures;

namespace GSDT.Architecture.Tests;

/// <summary>
/// Architecture enforcement tests for the InvestmentProjects module.
/// Verifies Clean/Onion layer dependency rules — Domain inward-only,
/// Application must not reach Infrastructure or Presentation.
///
/// Failure here means an architectural boundary was broken — fix the source, not the test.
/// </summary>
public sealed class InvestmentProjectsArchitectureTests
{
    // ── Domain layer isolation ────────────────────────────────────────────────

    [Fact]
    public void InvestmentProjects_Domain_ShouldNotDependOn_Application()
    {
        var result = Types
            .InAssembly(AssemblyFixtures.InvestmentProjectsDomain)
            .Should()
            .NotHaveDependencyOn("GSDT.InvestmentProjects.Application")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "InvestmentProjects Domain must not depend on Application (Clean Architecture — inward-only)");
    }

    [Fact]
    public void InvestmentProjects_Domain_ShouldNotDependOn_Infrastructure()
    {
        var result = Types
            .InAssembly(AssemblyFixtures.InvestmentProjectsDomain)
            .Should()
            .NotHaveDependencyOn("GSDT.InvestmentProjects.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "InvestmentProjects Domain must not depend on Infrastructure");
    }

    [Fact]
    public void InvestmentProjects_Domain_ShouldNotDependOn_Presentation()
    {
        var result = Types
            .InAssembly(AssemblyFixtures.InvestmentProjectsDomain)
            .Should()
            .NotHaveDependencyOn("GSDT.InvestmentProjects.Presentation")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "InvestmentProjects Domain must not depend on Presentation");
    }

    // ── Application layer isolation ───────────────────────────────────────────

    [Fact]
    public void InvestmentProjects_Application_ShouldNotDependOn_Infrastructure()
    {
        var result = Types
            .InAssembly(AssemblyFixtures.InvestmentProjectsApplication)
            .Should()
            .NotHaveDependencyOn("GSDT.InvestmentProjects.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "InvestmentProjects Application must not depend on Infrastructure — use interfaces/ports");
    }

    [Fact]
    public void InvestmentProjects_Application_ShouldNotDependOn_Presentation()
    {
        var result = Types
            .InAssembly(AssemblyFixtures.InvestmentProjectsApplication)
            .Should()
            .NotHaveDependencyOn("GSDT.InvestmentProjects.Presentation")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "InvestmentProjects Application must not depend on Presentation");
    }
}
