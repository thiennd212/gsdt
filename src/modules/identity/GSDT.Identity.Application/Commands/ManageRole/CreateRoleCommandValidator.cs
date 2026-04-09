using FluentValidation;

namespace GSDT.Identity.Application.Commands.ManageRole;

/// <summary>Validates CreateRoleCommand inputs before handler execution.</summary>
public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Mã vai trò không được để trống.")
            .MaximumLength(50).WithMessage("Mã vai trò không được vượt quá 50 ký tự.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên vai trò không được để trống.")
            .MaximumLength(200).WithMessage("Tên vai trò không được vượt quá 200 ký tự.");
    }
}
