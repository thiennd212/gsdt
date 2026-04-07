using FluentAssertions;
using FluentResults;
using MediatR;
using NSubstitute;
using GSDT.SharedKernel.Application;
using GSDT.SharedKernel.Application.Behaviors;
using GSDT.SharedKernel.Domain;

namespace GSDT.SharedKernel.Tests.Application.Behaviors;

// Namespace-level so Castle DynamicProxy can resolve it through strong-named assemblies
public sealed record TenantTestCommand : IBaseCommand;

/// <summary>
/// Tests TenantAwareBehavior: injects tenant context for commands, rejects missing tenant.
/// TC-SK-A003, TC-SK-A004
/// </summary>
public sealed class TenantAwareBehaviorTests
{

    private static ITenantContext MakeTenantContext(Guid? tenantId, bool isSystemAdmin = false)
    {
        var ctx = Substitute.For<ITenantContext>();
        ctx.TenantId.Returns(tenantId);
        ctx.IsSystemAdmin.Returns(isSystemAdmin);
        return ctx;
    }

    [Fact]
    public async Task Handle_CommandWithTenantId_CallsNext()
    {
        // TC-SK-A003: TenantAwareBehavior passes through when tenant is set
        var ctx = MakeTenantContext(Guid.NewGuid());
        var sut = new TenantAwareBehavior<TenantTestCommand, Result>(ctx);

        var nextCalled = false;
        RequestHandlerDelegate<Result> next = ct => { nextCalled = true; return Task.FromResult(Result.Ok()); };

        var result = await sut.Handle(new TenantTestCommand(), next, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_CommandWithMissingTenant_ReturnsFail()
    {
        // TC-SK-A004: TenantAwareBehavior rejects missing tenant
        var ctx = MakeTenantContext(null, isSystemAdmin: false);
        var sut = new TenantAwareBehavior<TenantTestCommand, Result>(ctx);

        var nextCalled = false;
        RequestHandlerDelegate<Result> next = ct => { nextCalled = true; return Task.FromResult(Result.Ok()); };

        var result = await sut.Handle(new TenantTestCommand(), next, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_SystemAdminWithNoTenant_CallsNext()
    {
        // SystemAdmin bypasses tenant check
        var ctx = MakeTenantContext(null, isSystemAdmin: true);
        var sut = new TenantAwareBehavior<TenantTestCommand, Result>(ctx);

        var nextCalled = false;
        RequestHandlerDelegate<Result> next = ct => { nextCalled = true; return Task.FromResult(Result.Ok()); };

        var result = await sut.Handle(new TenantTestCommand(), next, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_FailedResult_ContainsTenantErrorMessage()
    {
        var ctx = MakeTenantContext(null, isSystemAdmin: false);
        var sut = new TenantAwareBehavior<TenantTestCommand, Result>(ctx);

        RequestHandlerDelegate<Result> next = ct => Task.FromResult(Result.Ok());
        var result = await sut.Handle(new TenantTestCommand(), next, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().NotBeEmpty();
        result.Errors[0].Message.Should().Contain("Tenant");
    }
}
