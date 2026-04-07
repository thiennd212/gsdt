using GSDT.Architecture.Tests.Fixtures;
using FluentAssertions;
using NetArchTest.Rules;

namespace GSDT.Architecture.Tests;

/// <summary>
/// Architecture enforcement tests using NetArchTest.Rules.
/// Verifies Clean/Onion layer dependency rules and cross-module isolation.
/// Failure here means an architectural boundary was broken — fix the source, not the test.
///
/// Coverage is data-driven via AssemblyFixtures.Modules — adding a module there
/// automatically enrolls it in all layer + cross-module + convention checks.
/// </summary>
public sealed class ModuleIsolationTests
{
    // =========================================================
    // Layer isolation — Domain/Application must not reach up
    // =========================================================

    [Fact]
    public void AllModules_DomainLayer_Should_Not_Reference_Infrastructure()
    {
        foreach (var module in AssemblyFixtures.Modules)
        {
            var result = Types
                .InAssembly(module.Domain)
                .Should()
                .NotHaveDependencyOn($"GSDT.{module.Name}.Infrastructure")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                because: $"{module.Name} Domain must not depend on Infrastructure (Clean Architecture)");
        }
    }

    [Fact]
    public void AllModules_DomainLayer_Should_Not_Reference_Presentation()
    {
        foreach (var module in AssemblyFixtures.Modules)
        {
            var result = Types
                .InAssembly(module.Domain)
                .Should()
                .NotHaveDependencyOn($"GSDT.{module.Name}.Presentation")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                because: $"{module.Name} Domain must not depend on Presentation");
        }
    }

    [Fact]
    public void AllModules_ApplicationLayer_Should_Not_Reference_Infrastructure()
    {
        foreach (var module in AssemblyFixtures.Modules)
        {
            var result = Types
                .InAssembly(module.Application)
                .Should()
                .NotHaveDependencyOn($"GSDT.{module.Name}.Infrastructure")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                because: $"{module.Name} Application must not depend on Infrastructure — use interfaces/ports");
        }
    }

    [Fact]
    public void AllModules_ApplicationLayer_Should_Not_Reference_Presentation()
    {
        foreach (var module in AssemblyFixtures.Modules)
        {
            var result = Types
                .InAssembly(module.Application)
                .Should()
                .NotHaveDependencyOn($"GSDT.{module.Name}.Presentation")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                because: $"{module.Name} Application must not depend on Presentation");
        }
    }

    // =========================================================
    // Cross-module isolation — modules must not directly reference each other
    // (SharedKernel interfaces like ICurrentUser are the allowed contract)
    // =========================================================

    [Fact]
    public void NoModule_Should_DirectlyReference_AnotherModule()
    {
        foreach (var source in AssemblyFixtures.Modules)
        {
            // Build list of all OTHER module namespaces (Domain/Application/Infrastructure)
            var otherModuleNamespaces = AssemblyFixtures.Modules
                .Where(m => m.Name != source.Name)
                .SelectMany(m => new[]
                {
                    $"GSDT.{m.Name}.Domain",
                    $"GSDT.{m.Name}.Application",
                    $"GSDT.{m.Name}.Infrastructure"
                })
                .ToArray();

            // Check all 4 layers — modules must not couple to each other's internals
            var sourceAssemblies = new[] { source.Domain, source.Application, source.Infrastructure, source.Presentation };

            foreach (var assembly in sourceAssemblies)
            {
                var result = Types
                    .InAssembly(assembly)
                    .Should()
                    .NotHaveDependencyOnAny(otherModuleNamespaces)
                    .GetResult();

                result.IsSuccessful.Should().BeTrue(
                    because: $"{assembly.GetName().Name} must not directly reference other modules — use SharedKernel contracts");
            }
        }
    }

    // =========================================================
    // Controller conventions — all controllers extend ApiControllerBase
    // =========================================================

    [Fact]
    public void AllModules_Controllers_Should_InheritApiControllerBase()
    {
        foreach (var module in AssemblyFixtures.Modules)
        {
            var result = Types
                .InAssembly(module.Presentation)
                .That()
                .HaveNameEndingWith("Controller")
                .Should()
                .Inherit(typeof(GSDT.SharedKernel.Presentation.ApiControllerBase))
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                because: $"All {module.Name} controllers must extend ApiControllerBase for consistent ApiResponse<T> envelope");
        }
    }

    // =========================================================
    // Naming conventions
    // =========================================================

    [Fact]
    public void AllModules_CommandHandlers_Should_HaveHandlerSuffix()
    {
        foreach (var module in AssemblyFixtures.Modules)
        {
            var result = Types
                .InAssembly(module.Application)
                .That()
                .ImplementInterface(typeof(MediatR.IRequestHandler<,>))
                .Should()
                .HaveNameEndingWith("Handler")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                because: $"{module.Name} Command/Query handlers must be named *Handler by convention");
        }
    }

    [Fact]
    public void AllModules_DomainEntities_Should_ResideInEntitiesNamespace()
    {
        foreach (var module in AssemblyFixtures.Modules)
        {
            var result = Types
                .InAssembly(module.Domain)
                .That()
                .Inherit(typeof(GSDT.SharedKernel.Domain.AuditableEntity<>))
                .Should()
                .ResideInNamespaceContaining("Entities")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                because: $"{module.Name} domain entities should live in the Entities namespace");
        }
    }

    [Fact]
    public void AllModules_DomainExceptions_Should_InheritFromSystemException()
    {
        foreach (var module in AssemblyFixtures.Modules)
        {
            var result = Types
                .InAssembly(module.Domain)
                .That()
                .HaveNameEndingWith("Exception")
                .Should()
                .Inherit(typeof(Exception))
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                because: $"{module.Name} exceptions must inherit from System.Exception");
        }
    }

    // =========================================================
    // SharedKernel must remain infrastructure-free
    // =========================================================

    [Fact]
    public void SharedKernel_Should_Not_Reference_EntityFrameworkCore()
    {
        var result = Types
            .InAssembly(AssemblyFixtures.SharedKernel)
            .Should()
            .NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "SharedKernel is infrastructure-agnostic — no EF Core references allowed");
    }

    [Fact]
    public void SharedKernel_Should_Not_Reference_Dapper()
    {
        var result = Types
            .InAssembly(AssemblyFixtures.SharedKernel)
            .Should()
            .NotHaveDependencyOn("Dapper")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
