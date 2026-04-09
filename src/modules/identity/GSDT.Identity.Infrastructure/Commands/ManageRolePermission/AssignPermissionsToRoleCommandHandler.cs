using FluentResults;
using MediatR;
using GSDT.Identity.Application.Commands.ManageRolePermission;
using GSDT.Identity.Application.Queries.GetPermissions;
using GSDT.Identity.Application.Queries.GetRolePermissions;

namespace GSDT.Identity.Infrastructure.Commands.ManageRolePermission;

/// <summary>
/// Assigns permissions to a role with idempotency and cache invalidation.
/// Steps:
///   1. Verify role exists.
///   2. Verify all PermissionIds exist — fail if any are missing.
///   3. Insert only NEW assignments (skip already-assigned ones).
///   4. Invalidate effective-permission cache for all affected users.
///   5. Return updated permission list for the role.
/// </summary>
public sealed class AssignPermissionsToRoleCommandHandler(
    IdentityDbContext db,
    IEffectivePermissionService permissionService,
    IPermissionVersionService versionService)
    : IRequestHandler<AssignPermissionsToRoleCommand, Result<IReadOnlyList<PermissionDto>>>
{
    public async Task<Result<IReadOnlyList<PermissionDto>>> Handle(
        AssignPermissionsToRoleCommand cmd,
        CancellationToken ct)
    {
        // 1. Verify role exists
        var roleExists = await db.Roles.AnyAsync(r => r.Id == cmd.RoleId, ct);
        if (!roleExists)
            return Result.Fail(new NotFoundError($"Role {cmd.RoleId} not found."));

        // 2. Verify all requested permission IDs exist
        var existingPermIds = await db.Permissions
            .Where(p => cmd.PermissionIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync(ct);

        if (existingPermIds.Count != cmd.PermissionIds.Count)
        {
            var missing = cmd.PermissionIds.Except(existingPermIds).ToList();
            return Result.Fail(new ValidationError(
                $"Permission(s) not found: {string.Join(", ", missing)}",
                nameof(cmd.PermissionIds)));
        }

        // 3. Find already-assigned permission IDs to skip (idempotent)
        var alreadyAssigned = await db.RolePermissions
            .Where(rp => rp.RoleId == cmd.RoleId && cmd.PermissionIds.Contains(rp.PermissionId))
            .Select(rp => rp.PermissionId)
            .ToListAsync(ct);

        var toAdd = cmd.PermissionIds
            .Except(alreadyAssigned)
            .Select(permId => new RolePermission { RoleId = cmd.RoleId, PermissionId = permId })
            .ToList();

        if (toAdd.Count > 0)
        {
            db.RolePermissions.AddRange(toAdd);
            await db.SaveChangesAsync(ct);
        }

        // 4. Invalidate cache for all users who hold this role directly or via groups
        await RolePermissionCacheInvalidator.InvalidateForRoleUsersAsync(db, permissionService, versionService, cmd.RoleId, ct);

        // 5. Return updated permission list for the role
        var updated = await db.RolePermissions
            .Where(rp => rp.RoleId == cmd.RoleId)
            .Join(db.Permissions,
                rp => rp.PermissionId,
                p  => p.Id,
                (rp, p) => new PermissionDto(p.Id, p.Code, p.Name, p.Description, p.ModuleCode, p.ResourceCode, p.ActionCode))
            .OrderBy(p => p.ModuleCode).ThenBy(p => p.Code)
            .ToListAsync(ct);

        return Result.Ok<IReadOnlyList<PermissionDto>>(updated);
    }
}
