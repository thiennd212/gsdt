using FluentResults;
using MediatR;

namespace GSDT.Identity.Infrastructure.Commands.ManageDataScope;

/// <summary>
/// Creates a RoleDataScope assignment. Invalidates permission cache for all users with the role.
/// </summary>
public sealed class CreateRoleDataScopeCommandHandler(
    IdentityDbContext db,
    IEffectivePermissionService permissionService)
    : IRequestHandler<CreateRoleDataScopeCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateRoleDataScopeCommand cmd, CancellationToken ct)
    {
        var roleExists = await db.Roles.AnyAsync(r => r.Id == cmd.RoleId, ct);
        if (!roleExists)
            return Result.Fail(new NotFoundError($"Role {cmd.RoleId} not found."));

        var scopeTypeExists = await db.DataScopeTypes.AnyAsync(t => t.Id == cmd.DataScopeTypeId, ct);
        if (!scopeTypeExists)
            return Result.Fail(new NotFoundError($"DataScopeType {cmd.DataScopeTypeId} not found."));

        var scope = new RoleDataScope
        {
            Id = Guid.NewGuid(),
            RoleId = cmd.RoleId,
            DataScopeTypeId = cmd.DataScopeTypeId,
            ScopeField = cmd.ScopeField,
            ScopeValue = cmd.ScopeValue,
            Priority = cmd.Priority
        };

        db.RoleDataScopes.Add(scope);
        await db.SaveChangesAsync(ct);

        // Invalidate cache for all users bearing this role
        var userIds = await db.UserRoles
            .Where(ur => ur.RoleId == cmd.RoleId)
            .Select(ur => ur.UserId)
            .ToListAsync(ct);

        foreach (var uid in userIds)
            await permissionService.InvalidateAsync(uid);

        return Result.Ok(scope.Id);
    }
}

/// <summary>Deletes a RoleDataScope assignment by ID.</summary>
public sealed class DeleteRoleDataScopeCommandHandler(
    IdentityDbContext db,
    IEffectivePermissionService permissionService)
    : IRequestHandler<DeleteRoleDataScopeCommand, Result>
{
    public async Task<Result> Handle(DeleteRoleDataScopeCommand cmd, CancellationToken ct)
    {
        var scope = await db.RoleDataScopes.FindAsync([cmd.ScopeId], ct);
        if (scope is null)
            return Result.Fail(new NotFoundError($"RoleDataScope {cmd.ScopeId} not found."));

        var roleId = scope.RoleId;
        db.RoleDataScopes.Remove(scope);
        await db.SaveChangesAsync(ct);

        var userIds = await db.UserRoles
            .Where(ur => ur.RoleId == roleId)
            .Select(ur => ur.UserId)
            .ToListAsync(ct);

        foreach (var uid in userIds)
            await permissionService.InvalidateAsync(uid);

        return Result.Ok();
    }
}
