using FluentValidation;

namespace GSDT.Identity.Application.Commands.ManageRolePermission;

/// <summary>
/// Removes specified permissions from a role.
/// Non-assigned permissions are silently skipped (idempotent, not an error).
/// </summary>
public sealed record RemovePermissionsFromRoleCommand(
    Guid RoleId,
    IReadOnlyList<Guid> PermissionIds) : ICommand;

/// <summary>Validates RemovePermissionsFromRoleCommand — enforces bulk limit to prevent DoS.</summary>
public sealed class RemovePermissionsFromRoleCommandValidator : AbstractValidator<RemovePermissionsFromRoleCommand>
{
    public RemovePermissionsFromRoleCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("RoleId không được để trống.");

        RuleFor(x => x.PermissionIds)
            .NotEmpty().WithMessage("Danh sách quyền không được rỗng.")
            .Must(ids => ids.Count <= 100)
                .WithMessage("Không được xóa quá 100 quyền trong một lần.");
    }
}
