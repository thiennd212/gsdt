using FluentResults;
using MediatR;

namespace GSDT.Identity.Application.Commands.SyncUserRoles;

/// <summary>
/// Replaces user roles: removes roles not in the provided list, adds missing ones.
/// Uses UserManager.AddToRolesAsync/RemoveFromRolesAsync for atomic operation.
/// </summary>
public sealed class SyncUserRolesCommandHandler(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    ICacheService cache,
    IMediator mediator) : IRequestHandler<SyncUserRolesCommand, Result>
{
    public async Task<Result> Handle(SyncUserRolesCommand cmd, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(cmd.UserId.ToString());
        if (user is null)
            return Result.Fail(new NotFoundError($"User {cmd.UserId} not found"));

        // Validate all requested roles exist
        foreach (var role in cmd.Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                return Result.Fail(new NotFoundError($"Role '{role}' not found"));
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        var rolesToRemove = currentRoles.Except(cmd.Roles).ToList();
        var rolesToAdd = cmd.Roles.Except(currentRoles).ToList();

        if (rolesToRemove.Count > 0)
        {
            var removeResult = await userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
                return Result.Fail(removeResult.Errors.Select(e => new ValidationError(e.Description)));
        }

        if (rolesToAdd.Count > 0)
        {
            var addResult = await userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addResult.Succeeded)
                return Result.Fail(addResult.Errors.Select(e => new ValidationError(e.Description)));
        }

        // Invalidate role cache
        await cache.RemoveAsync($"user-roles:{cmd.UserId}");

        // Revoke all active tokens — forces re-login with updated claims (C-02 security fix)
        if (rolesToRemove.Count > 0 || rolesToAdd.Count > 0)
        {
            await mediator.Send(new RevokeTokenCommand(TokenId: null, UserId: cmd.UserId, ActorId: cmd.UserId), ct);
        }

        return Result.Ok();
    }
}
