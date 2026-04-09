namespace GSDT.Identity.Infrastructure.Commands.ManageRolePermission;

/// <summary>
/// Shared helper for invalidating effective-permission caches after role-permission mutations.
/// Called by both AssignPermissionsToRoleCommandHandler and RemovePermissionsFromRoleCommandHandler.
/// Covers direct user-role assignments (AspNetUserRoles) and indirect group-based assignments.
/// </summary>
internal static class RolePermissionCacheInvalidator
{
    /// <summary>
    /// Finds all users who hold <paramref name="roleId"/> (directly or via group membership)
    /// and invalidates their effective-permission cache + increments their permission version.
    /// </summary>
    internal static async Task InvalidateForRoleUsersAsync(
        IdentityDbContext db,
        IEffectivePermissionService permissionService,
        IPermissionVersionService versionService,
        Guid roleId,
        CancellationToken ct)
    {
        // Direct role assignments via AspNetUserRoles
        var directUserIds = await db.UserRoles
            .Where(ur => ur.RoleId == roleId)
            .Select(ur => ur.UserId)
            .ToListAsync(ct);

        // Indirect via groups: GroupRoleAssignments → UserGroupMemberships
        var groupIds = await db.GroupRoleAssignments
            .Where(gra => gra.RoleId == roleId)
            .Select(gra => gra.GroupId)
            .ToListAsync(ct);

        var groupUserIds = await db.UserGroupMemberships
            .Where(m => groupIds.Contains(m.GroupId))
            .Select(m => m.UserId)
            .ToListAsync(ct);

        var allUserIds = directUserIds.Union(groupUserIds).Distinct();

        foreach (var userId in allUserIds)
        {
            await permissionService.InvalidateAsync(userId);
            await versionService.IncrementAsync(userId);
        }
    }
}
