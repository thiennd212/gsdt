using FluentValidation;
using GSDT.Identity.Application.Queries.GetPermissions;

namespace GSDT.Identity.Application.Commands.ManageRolePermission;

/// <summary>
/// Assigns a list of permissions to a role. Idempotent — already-assigned permissions are skipped.
/// Returns the full updated permission list for the role after assignment.
/// </summary>
public sealed record AssignPermissionsToRoleCommand(
    Guid RoleId,
    IReadOnlyList<Guid> PermissionIds) : ICommand<IReadOnlyList<PermissionDto>>;

/// <summary>Validates AssignPermissionsToRoleCommand inputs before handler execution.</summary>
public sealed class AssignPermissionsToRoleCommandValidator : AbstractValidator<AssignPermissionsToRoleCommand>
{
    public AssignPermissionsToRoleCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("RoleId không được để trống.");

        RuleFor(x => x.PermissionIds)
            .NotEmpty().WithMessage("Danh sách quyền không được rỗng.")
            .Must(ids => ids.Count <= 100)
                .WithMessage("Không được gán quá 100 quyền trong một lần.");
    }
}
