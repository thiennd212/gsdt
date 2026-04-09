using FluentResults;
using GSDT.SharedKernel.Errors;
using MediatR;

namespace GSDT.Identity.Application.Commands.ManageRole;

/// <summary>
/// Handles DeleteRoleCommand — soft-deletes a business role (sets IsActive = false).
/// Rejects System roles unconditionally to protect built-in access control entries.
/// </summary>
public sealed class DeleteRoleCommandHandler(RoleManager<ApplicationRole> roleManager)
    : IRequestHandler<DeleteRoleCommand, Result>
{
    public async Task<Result> Handle(DeleteRoleCommand cmd, CancellationToken ct)
    {
        var role = await roleManager.FindByIdAsync(cmd.Id.ToString());

        if (role is null)
            return Result.Fail(new NotFoundError($"Vai trò '{cmd.Id}' không tồn tại."));

        if (role.RoleType == RoleType.System)
            return Result.Fail(new ValidationError("Không thể xóa vai trò hệ thống.", "RoleType"));

        role.IsActive = false;

        var identityResult = await roleManager.UpdateAsync(role);

        if (!identityResult.Succeeded)
        {
            var errors = string.Join("; ", identityResult.Errors.Select(e => e.Description));
            return Result.Fail($"Không thể xóa vai trò: {errors}");
        }

        return Result.Ok();
    }
}
