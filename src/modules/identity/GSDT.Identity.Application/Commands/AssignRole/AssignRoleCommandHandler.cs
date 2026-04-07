using FluentResults;
using MediatR;

namespace GSDT.Identity.Application.Commands.AssignRole;

public sealed class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ICacheService _cache;
    private readonly IMediator _mediator;

    public AssignRoleCommandHandler(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ICacheService cache,
        IMediator mediator)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _cache = cache;
        _mediator = mediator;
    }

    public async Task<Result> Handle(AssignRoleCommand cmd, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(cmd.UserId.ToString());
        if (user is null)
            return Result.Fail(new NotFoundError($"User {cmd.UserId} not found"));

        if (!await _roleManager.RoleExistsAsync(cmd.RoleName))
            return Result.Fail(new NotFoundError($"Role '{cmd.RoleName}' not found"));

        var identityResult = await _userManager.AddToRoleAsync(user, cmd.RoleName);
        if (!identityResult.Succeeded)
            return Result.Fail(identityResult.Errors.Select(e => new ValidationError(e.Description)));

        // Invalidate role cache so ClaimsEnrichmentTransformer picks up change immediately
        await _cache.RemoveAsync($"user-roles:{cmd.UserId}");

        // Revoke all active tokens — forces re-login with updated claims (C-02 security fix)
        await _mediator.Send(new RevokeTokenCommand(TokenId: null, UserId: cmd.UserId, ActorId: cmd.UserId), ct);

        return Result.Ok();
    }
}
