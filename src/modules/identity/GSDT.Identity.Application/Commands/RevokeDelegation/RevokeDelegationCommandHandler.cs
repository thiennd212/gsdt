using FluentResults;
using MediatR;

namespace GSDT.Identity.Application.Commands.RevokeDelegation;

public sealed class RevokeDelegationCommandHandler : IRequestHandler<RevokeDelegationCommand, Result>
{
    private readonly IDelegationRepository _delegations;
    private readonly ICacheService _cache;

    public RevokeDelegationCommandHandler(
        IDelegationRepository delegations,
        ICacheService cache,
    {
        _delegations = delegations;
        _cache = cache;
        _events = events;
    }

    public async Task<Result> Handle(RevokeDelegationCommand cmd, CancellationToken ct)
    {
        var delegation = await _delegations.GetByIdAsync(cmd.DelegationId, ct);

        if (delegation is null)
            return Result.Fail(new NotFoundError($"Delegation {cmd.DelegationId} not found."));

        if (delegation.IsRevoked)
            return Result.Fail(new ConflictError($"Delegation {cmd.DelegationId} is already revoked."));

        delegation.IsRevoked = true;
        delegation.RevokedBy = cmd.ActorId;

        await _delegations.SaveChangesAsync(ct);

        // Invalidate delegation cache so ClaimsEnrichmentTransformer picks up change within TTL
        await _cache.RemoveAsync($"delegation:{delegation.DelegateId}", ct);

        await _events.PublishAsync(
            [new DelegationRevokedEvent(cmd.DelegationId, delegation.DelegatorId, delegation.DelegateId, cmd.ActorId)], ct);

        return Result.Ok();
    }
}
