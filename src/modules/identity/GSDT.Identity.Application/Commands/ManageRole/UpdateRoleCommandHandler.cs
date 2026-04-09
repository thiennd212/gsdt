using FluentResults;
using GSDT.SharedKernel.Errors;
using MediatR;
using GSDT.Identity.Application.Queries.GetRoleById;

namespace GSDT.Identity.Application.Commands.ManageRole;

/// <summary>
/// Handles UpdateRoleCommand — updates Name + Description.
/// System roles: Description only — Name is immutable to protect built-in permissions.
/// </summary>
public sealed class UpdateRoleCommandHandler(RoleManager<ApplicationRole> roleManager)
    : IRequestHandler<UpdateRoleCommand, Result<RoleDetailDto>>
{
    public async Task<Result<RoleDetailDto>> Handle(UpdateRoleCommand cmd, CancellationToken ct)
    {
        var role = await roleManager.FindByIdAsync(cmd.Id.ToString());

        if (role is null)
            return Result.Fail(new NotFoundError($"Vai trò '{cmd.Id}' không tồn tại."));

        // System roles: only Description is mutable; Name is locked to prevent
        // breaking permission checks that may rely on the normalized name.
        if (role.RoleType != RoleType.System)
            role.Name = cmd.Name;

        role.Description = cmd.Description;

        var identityResult = await roleManager.UpdateAsync(role);

        if (!identityResult.Succeeded)
        {
            var errors = string.Join("; ", identityResult.Errors.Select(e => e.Description));
            return Result.Fail($"Không thể cập nhật vai trò: {errors}");
        }

        // C1 fix: RoleManager.FindByIdAsync does NOT eager-load navigation properties.
        // Return empty permissions — FE calls GetRoleById for full detail with permissions.
        var dto = new RoleDetailDto(
            role.Id,
            role.Code,
            role.Name ?? string.Empty,
            role.Description,
            role.RoleType.ToString(),
            role.IsActive,
            Array.Empty<RolePermissionDto>());

        return Result.Ok(dto);
    }
}
