using FluentResults;
using MediatR;
using GSDT.Identity.Application.Commands.ManageRolePermission;

namespace GSDT.Identity.Infrastructure.Commands.ManageRolePermission;

/// <summary>
/// Removes permissions from a role. Non-assigned permissions are silently skipped (idempotent).
/// Invalidates effective-permission cache for all affected users after removal.
/// </summary>
public sealed class RemovePermissionsFromRoleCommandHandler(
    IdentityDbContext db,
    IEffectivePermissionService permissionService,
    IPermissionVersionService versionService)
    : IRequestHandler<RemovePermissionsFromRoleCommand, Result>
{
    public async Task<Result> Handle(
        RemovePermissionsFromRoleCommand cmd,
        CancellationToken ct)
    {
        // 1. Find the existing RolePermission rows to remove (non-assigned = no-op)
        var toRemove = await db.RolePermissions
            .Where(rp => rp.RoleId == cmd.RoleId && cmd.PermissionIds.Contains(rp.PermissionId))
            .ToListAsync(ct);

        if (toRemove.Count > 0)
        {
            db.RolePermissions.RemoveRange(toRemove);
            await db.SaveChangesAsync(ct);
        }

        // 2. Invalidate cache for all users who hold this role directly or via groups
        await RolePermissionCacheInvalidator.InvalidateForRoleUsersAsync(db, permissionService, versionService, cmd.RoleId, ct);

        return Result.Ok();
    }
}
