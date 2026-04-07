using FluentResults;
using MediatR;

namespace GSDT.Identity.Infrastructure.Commands.ManageGroup;

/// <summary>
/// Assigns a role to a group. Idempotent — returns Ok if assignment already exists.
/// Invalidates effective-permission cache for all current members of the group.
/// </summary>
public sealed class AssignRoleToGroupCommandHandler(
    IdentityDbContext db,
    IEffectivePermissionService permissionService)
    : IRequestHandler<AssignRoleToGroupCommand, Result>
{
    public async Task<Result> Handle(AssignRoleToGroupCommand cmd, CancellationToken ct)
    {
        var groupExists = await db.UserGroups.AnyAsync(g => g.Id == cmd.GroupId, ct);
        if (!groupExists)
            return Result.Fail(new NotFoundError($"Group {cmd.GroupId} not found."));

        var roleExists = await db.Roles.AnyAsync(r => r.Id == cmd.RoleId, ct);
        if (!roleExists)
            return Result.Fail(new NotFoundError($"Role {cmd.RoleId} not found."));

        // Idempotent — skip if already assigned
        var alreadyAssigned = await db.GroupRoleAssignments
            .AnyAsync(a => a.GroupId == cmd.GroupId && a.RoleId == cmd.RoleId, ct);

        if (!alreadyAssigned)
        {
            db.GroupRoleAssignments.Add(new GroupRoleAssignment
            {
                Id = Guid.NewGuid(),
                GroupId = cmd.GroupId,
                RoleId = cmd.RoleId,
                AssignedBy = cmd.AssignedBy,
                AssignedAtUtc = DateTime.UtcNow
            });
            await db.SaveChangesAsync(ct);
        }

        // Invalidate cache for all members so the new role takes effect immediately
        var memberIds = await db.UserGroupMemberships
            .Where(m => m.GroupId == cmd.GroupId)
            .Select(m => m.UserId)
            .ToListAsync(ct);

        foreach (var userId in memberIds)
            await permissionService.InvalidateAsync(userId);

        return Result.Ok();
    }
}
