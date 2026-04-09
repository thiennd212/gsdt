using FluentValidation;

namespace GSDT.Identity.Application.Commands.ManageRole;

/// <summary>Validates UpdateRoleCommand inputs before handler execution.</summary>
public sealed class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID vai trò không được để trống.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên vai trò không được để trống.")
            .MaximumLength(200).WithMessage("Tên vai trò không được vượt quá 200 ký tự.");
    }
}
