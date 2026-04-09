using GSDT.Identity.Application.Commands.ManageRole;
using FluentValidation.TestHelper;
using Xunit;

namespace GSDT.Identity.Application.Tests.Commands.ManageRole;

/// <summary>
/// Validator tests for CreateRoleCommand.
/// Verifies: empty code, empty name, code too long (>50), valid input passes.
/// </summary>
public sealed class CreateRoleCommandValidatorTests
{
    private readonly CreateRoleCommandValidator _validator = new();

    [Fact]
    public void EmptyCode_Fails()
    {
        var cmd = new CreateRoleCommand("", "Valid Name", null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void EmptyName_Fails()
    {
        var cmd = new CreateRoleCommand("VALID_CODE", "", null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CodeTooLong_Fails()
    {
        var cmd = new CreateRoleCommand(new string('X', 51), "Valid Name", null);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void ValidInput_Passes()
    {
        var cmd = new CreateRoleCommand("CHUYEN_VIEN", "Chuyên viên xử lý", "Mô tả vai trò");
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
