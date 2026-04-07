using FluentResults;
using MediatR;

namespace GSDT.Identity.Application.Commands.DelegateRole;

public sealed class DelegateRoleCommandHandler : IRequestHandler<DelegateRoleCommand, Result<Guid>>
{
    private readonly IDelegationRepository _delegations;
    private readonly UserManager<ApplicationUser> _userManager;

    public DelegateRoleCommandHandler(
        IDelegationRepository delegations,
        UserManager<ApplicationUser> userManager)
    {
        _delegations = delegations;
        _events = events;
        _userManager = userManager;
    }

    public async Task<Result<Guid>> Handle(DelegateRoleCommand cmd, CancellationToken ct)
    {
        if (cmd.ValidFrom >= cmd.ValidTo)
            return Result.Fail(new ValidationError("ValidFrom must be before ValidTo."));

        if (cmd.ValidTo <= DateTime.UtcNow)
            return Result.Fail(new ValidationError("ValidTo must be in the future."));

        if (cmd.DelegatorId == cmd.DelegateId)
            return Result.Fail(new ValidationError("Cannot delegate to yourself."));

        // C-03: Block circular delegation — if B→A already exists, reject A→B
        var reverseExists = await _delegations.HasActiveOverlapAsync(
            cmd.DelegateId,     // reverse: delegate becomes delegator
            cmd.DelegatorId,    // reverse: delegator becomes delegate
            cmd.ValidFrom,
            cmd.ValidTo,
            ct);

        if (reverseExists)
            return Result.Fail(new ConflictError(
                "Circular delegation detected: the delegate already has an active delegation to the delegator. " +
                "Phát hiện ủy quyền vòng tròn: người được ủy quyền đã có ủy quyền ngược lại cho người ủy quyền."));

        // F-08: ownership check — actor can only create delegations for themselves unless Admin/SystemAdmin
        if (cmd.DelegatorId != cmd.ActorId)
        {
            var actor = await _userManager.FindByIdAsync(cmd.ActorId.ToString());
            if (actor is null)
                return Result.Fail(new ForbiddenError("Actor not found."));

            var actorRoles = await _userManager.GetRolesAsync(actor);
            if (!actorRoles.Any(r => r is "Admin" or "SystemAdmin"))
                return Result.Fail(new ForbiddenError("You can only create delegations for yourself."));
        }

        var hasDuplicate = await _delegations.HasActiveOverlapAsync(
            cmd.DelegatorId, cmd.DelegateId, cmd.ValidFrom, cmd.ValidTo, ct);

        if (hasDuplicate)
            return Result.Fail(new ConflictError(
                $"An active delegation from {cmd.DelegatorId} to {cmd.DelegateId} already overlaps the requested window."));

        var delegation = new UserDelegation
        {
            Id = Guid.NewGuid(),
            DelegatorId = cmd.DelegatorId,
            DelegateId = cmd.DelegateId,
            ValidFrom = cmd.ValidFrom,
            ValidTo = cmd.ValidTo,
            Reason = cmd.Reason,
            IsRevoked = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _delegations.AddAsync(delegation, ct);
        await _delegations.SaveChangesAsync(ct);

        await _events.PublishAsync(
            [new DelegationGrantedEvent(delegation.Id, cmd.DelegatorId, cmd.DelegateId, cmd.ActorId)], ct);

        return Result.Ok(delegation.Id);
    }
}
