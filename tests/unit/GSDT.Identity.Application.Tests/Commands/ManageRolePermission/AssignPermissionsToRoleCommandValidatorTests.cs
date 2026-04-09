using GSDT.Identity.Application.Commands.ManageRolePermission;
using FluentValidation.TestHelper;
using Xunit;

namespace GSDT.Identity.Application.Tests.Commands.ManageRolePermission;

/// <summary>
/// Validator tests for AssignPermissionsToRoleCommand.
/// Verifies: empty RoleId, empty PermissionIds, too many items (>100), valid input passes.
/// </summary>
public sealed class AssignPermissionsToRoleCommandValidatorTests
{
    private readonly AssignPermissionsToRoleCommandValidator _validator = new();

    [Fact]
    public void EmptyRoleId_Fails()
    {
        var cmd = new AssignPermissionsToRoleCommand(
            Guid.Empty,
            new List<Guid> { Guid.NewGuid() });

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.RoleId);
    }

    [Fact]
    public void EmptyPermissionIds_Fails()
    {
        var cmd = new AssignPermissionsToRoleCommand(
            Guid.NewGuid(),
            new List<Guid>());

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.PermissionIds);
    }

    [Fact]
    public void TooManyPermissionIds_Fails()
    {
        var ids = Enumerable.Range(0, 101).Select(_ => Guid.NewGuid()).ToList();
        var cmd = new AssignPermissionsToRoleCommand(Guid.NewGuid(), ids);

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.PermissionIds);
    }

    [Fact]
    public void ValidInput_Passes()
    {
        var ids = Enumerable.Range(0, 5).Select(_ => Guid.NewGuid()).ToList();
        var cmd = new AssignPermissionsToRoleCommand(Guid.NewGuid(), ids);

        var result = _validator.TestValidate(cmd);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
