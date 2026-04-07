using FluentResults;
using MediatR;

namespace GSDT.Identity.Infrastructure.Commands.ManageGroup;

/// <summary>
/// Adds a user to a group. Idempotent — returns Ok if membership already exists.
/// Invalidates effective-permission cache so group roles apply immediately.
/// </summary>
public sealed class AddUserToGroupCommandHandler(
    IdentityDbContext db,
    IEffectivePermissionService permissionService)
    : IRequestHandler<AddUserToGroupCommand, Result>
{
    public async Task<Result> Handle(AddUserToGroupCommand cmd, CancellationToken ct)
    {
        var groupExists = await db.UserGroups.AnyAsync(g => g.Id == cmd.GroupId, ct);
        if (!groupExists)
            return Result.Fail(new NotFoundError($"Group {cmd.GroupId} not found."));

        var userExists = await db.Users.AnyAsync(u => u.Id == cmd.UserId, ct);
        if (!userExists)
            return Result.Fail(new NotFoundError($"User {cmd.UserId} not found."));

        // Idempotent — skip if already a member
        var alreadyMember = await db.UserGroupMemberships
            .AnyAsync(m => m.GroupId == cmd.GroupId && m.UserId == cmd.UserId, ct);

        if (!alreadyMember)
        {
            db.UserGroupMemberships.Add(new UserGroupMembership
            {
                Id = Guid.NewGuid(),
                GroupId = cmd.GroupId,
                UserId = cmd.UserId,
                AddedBy = cmd.AddedBy,
                AddedAtUtc = DateTime.UtcNow
            });
            await db.SaveChangesAsync(ct);
        }

        // Invalidate cache so group-inherited roles take effect immediately
        await permissionService.InvalidateAsync(cmd.UserId);

        return Result.Ok();
    }
}
