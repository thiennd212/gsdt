using GSDT.Identity.Application.Commands.ManageRole;
using GSDT.Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using FluentAssertions;

namespace GSDT.Identity.Application.Tests.Commands.ManageRole;

/// <summary>
/// Handler tests for DeleteRoleCommand.
/// Verifies: business role soft-deleted, system role rejected, non-existent role fails.
/// </summary>
public sealed class DeleteRoleCommandHandlerTests
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly DeleteRoleCommandHandler _sut;

    public DeleteRoleCommandHandlerTests()
    {
        var store = Substitute.For<IRoleStore<ApplicationRole>>();
        _roleManager = Substitute.For<RoleManager<ApplicationRole>>(
            store,
            Array.Empty<IRoleValidator<ApplicationRole>>(),
            Substitute.For<ILookupNormalizer>(),
            Substitute.For<IdentityErrorDescriber>(),
            Substitute.For<ILogger<RoleManager<ApplicationRole>>>());

        _sut = new DeleteRoleCommandHandler(_roleManager);
    }

    [Fact]
    public async Task BusinessRole_SetsInactive()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = new ApplicationRole
        {
            Id = roleId,
            Code = "CHUYEN_VIEN",
            Name = "Chuyên viên",
            RoleType = RoleType.Business,
            IsActive = true
        };

        _roleManager.FindByIdAsync(roleId.ToString()).Returns(role);
        _roleManager.UpdateAsync(Arg.Any<ApplicationRole>()).Returns(IdentityResult.Success);

        var cmd = new DeleteRoleCommand(roleId);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _roleManager.Received(1).UpdateAsync(
            Arg.Is<ApplicationRole>(r => r.Id == roleId && r.IsActive == false));
    }

    [Fact]
    public async Task SystemRole_ReturnsFail()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = new ApplicationRole
        {
            Id = roleId,
            Code = "ADMIN",
            Name = "System Admin",
            RoleType = RoleType.System,
            IsActive = true
        };

        _roleManager.FindByIdAsync(roleId.ToString()).Returns(role);

        var cmd = new DeleteRoleCommand(roleId);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("hệ thống"));
        await _roleManager.DidNotReceive().UpdateAsync(Arg.Any<ApplicationRole>());
    }

    [Fact]
    public async Task NonExistentRole_ReturnsFail()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        _roleManager.FindByIdAsync(roleId.ToString()).Returns((ApplicationRole?)null);

        var cmd = new DeleteRoleCommand(roleId);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains(roleId.ToString()));
    }
}
