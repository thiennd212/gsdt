using GSDT.Identity.Application.Commands.ManageRole;
using GSDT.Identity.Application.Queries.GetRoleById;
using GSDT.Identity.Domain.Entities;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;
using FluentAssertions;

namespace GSDT.Identity.Application.Tests.Commands.ManageRole;

/// <summary>
/// Handler tests for CreateRoleCommand.
/// Verifies: success path returns RoleDetailDto, duplicate code returns fail.
/// </summary>
public sealed class CreateRoleCommandHandlerTests
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly CreateRoleCommandHandler _sut;

    public CreateRoleCommandHandlerTests()
    {
        var store = Substitute.For<IRoleStore<ApplicationRole>>();
        _roleManager = Substitute.For<RoleManager<ApplicationRole>>(
            store,
            Array.Empty<IRoleValidator<ApplicationRole>>(),
            Substitute.For<ILookupNormalizer>(),
            Substitute.For<IdentityErrorDescriber>(),
            Substitute.For<ILogger<RoleManager<ApplicationRole>>>());

        _sut = new CreateRoleCommandHandler(_roleManager);
    }

    [Fact]
    public async Task ValidCommand_CreatesRole_ReturnDto()
    {
        // Arrange
        var cmd = new CreateRoleCommand("CHUYEN_VIEN", "Chuyên viên xử lý", "Mô tả vai trò");

        // No existing role with that code
        _roleManager.Roles.Returns(new List<ApplicationRole>().AsQueryable());
        _roleManager.CreateAsync(Arg.Any<ApplicationRole>())
            .Returns(IdentityResult.Success);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Code.Should().Be("CHUYEN_VIEN");
        result.Value.Name.Should().Be("Chuyên viên xử lý");
        result.Value.Description.Should().Be("Mô tả vai trò");
    }

    [Fact]
    public async Task DuplicateCode_ReturnsFail()
    {
        // Arrange
        var cmd = new CreateRoleCommand("CHUYEN_VIEN", "Chuyên viên", null);

        var existingRole = new ApplicationRole { Code = "CHUYEN_VIEN", Name = "Existing" };
        _roleManager.Roles.Returns(new List<ApplicationRole> { existingRole }.AsQueryable());

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e.Message.Contains("CHUYEN_VIEN"));
    }
}
