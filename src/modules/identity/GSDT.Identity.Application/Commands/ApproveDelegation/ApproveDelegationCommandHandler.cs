using FluentResults;
using MediatR;

namespace GSDT.Identity.Application.Commands.ApproveDelegation;

/// <summary>
/// Transitions a PendingApproval delegation to Active.
/// Enforces Admin/SystemAdmin role on the actor.
/// Invalidates delegation Redis cache for the delegate so ClaimsEnrichmentTransformer
/// picks up the newly-active delegation within its TTL.
/// </summary>
public sealed class ApproveDelegationCommandHandler : IRequestHandler<ApproveDelegationCommand, Result>
{
    private readonly IDelegationRepository _delegations;
    private readonly ICacheService _cache;
    private readonly UserManager<ApplicationUser> _userManager;

    public ApproveDelegationCommandHandler(
        IDelegationRepository delegations,
        ICacheService cache,
        UserManager<ApplicationUser> userManager)
    {
        _delegations = delegations;
        _cache = cache;
        _userManager = userManager;
    }

    public async Task<Result> Handle(ApproveDelegationCommand cmd, CancellationToken ct)
    {
        // Enforce Admin/SystemAdmin — only privileged users can approve
        var actor = await _userManager.FindByIdAsync(cmd.ActorId.ToString());
        if (actor is null)
            return Result.Fail(new ForbiddenError("Actor not found."));

        var actorRoles = await _userManager.GetRolesAsync(actor);
        if (!actorRoles.Any(r => r is "Admin" or "SystemAdmin"))
            return Result.Fail(new ForbiddenError("Only Admin or SystemAdmin can approve delegations."));

        var delegation = await _delegations.GetByIdAsync(cmd.DelegationId, ct);
        if (delegation is null)
            return Result.Fail(new NotFoundError($"Delegation {cmd.DelegationId} not found."));

        if (delegation.Status != DelegationStatus.PendingApproval)
            return Result.Fail(new ConflictError(
                $"Delegation {cmd.DelegationId} is not in PendingApproval state (current: {delegation.Status})."));

        delegation.Status = DelegationStatus.Active;
        delegation.ApprovedBy = cmd.ActorId;
        delegation.ApprovedAtUtc = DateTime.UtcNow;

        await _delegations.SaveChangesAsync(ct);

        // Invalidate delegation cache so claims transformer picks up new Active delegation
        await _cache.RemoveAsync($"delegation:{delegation.DelegateId}", ct);

        return Result.Ok();
    }
}
