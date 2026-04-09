using GSDT.Identity.Application.Commands.ManageRole;
using GSDT.Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using FluentAssertions;

namespace GSDT.Identity.Application.Tests.Commands.ManageRole;

/// <summary>
/// Handler tests for UpdateRoleCommand.
/// Verifies: valid update, non-existent role returns fail, system role cannot rename Name.
/// </summary>
public sealed class UpdateRoleCommandHandlerTests
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UpdateRoleCommandHandler _sut;

    public UpdateRoleCommandHandlerTests()
    {
        var store = Substitute.For<IRoleStore<ApplicationRole>>();
        _roleManager = Substitute.For<RoleManager<ApplicationRole>>(
            store,
            Array.Empty<IRoleValidator<ApplicationRole>>(),
            Substitute.For<ILookupNormalizer>(),
            Substitute.For<IdentityErrorDescriber>(),
            Substitute.For<ILogger<RoleManager<ApplicationRole>>>());

        _sut = new UpdateRoleCommandHandler(_roleManager);
    }

    [Fact]
    public async Task ValidCommand_UpdatesNameAndDescription()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var role = new ApplicationRole
        {
            Id = roleId,
            Code = "CHUYEN_VIEN",
            Name = "Old Name",
            Description = "Old Desc",
            RoleType = RoleType.Business,
            IsActive = true
        };

        _roleManager.FindByIdAsync(roleId.ToString()).Returns(role);
        _roleManager.UpdateAsync(Arg.Any<ApplicationRole>()).Returns(IdentityResult.Success);

        var cmd = new UpdateRoleCommand(roleId, "New Name", "New Desc");

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("New Name");
        result.Value.Description.Should().Be("New Desc");
    }

    [Fact]
    public async Task NonExistentRole_ReturnsFail()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        _roleManager.FindByIdAsync(roleId.ToString()).Returns((ApplicationRole?)null);

        var cmd = new UpdateRoleCommand(roleId, "Some Name", null);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains(roleId.ToString()));
    }

    [Fact]
    public async Task SystemRole_CanUpdateDescription_CannotRenameName()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var originalName = "System Admin";
        var role = new ApplicationRole
        {
            Id = roleId,
            Code = "ADMIN",
            Name = originalName,
            Description = "Old system desc",
            RoleType = RoleType.System,
            IsActive = true
        };

        _roleManager.FindByIdAsync(roleId.ToString()).Returns(role);
        _roleManager.UpdateAsync(Arg.Any<ApplicationRole>()).Returns(IdentityResult.Success);

        var cmd = new UpdateRoleCommand(roleId, "New Admin Name", "Updated description");

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert — Description updated, Name unchanged for system role
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(originalName);
        result.Value.Description.Should().Be("Updated description");
    }
}
