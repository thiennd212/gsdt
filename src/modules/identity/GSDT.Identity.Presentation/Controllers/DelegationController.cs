using MediatR;

namespace GSDT.Identity.Presentation.Controllers;

/// <summary>
/// Role delegation endpoints — any authenticated user can delegate or view their delegations.
/// Approve restricted to Admin/SystemAdmin (enforced in handler).
/// Revocation restricted to the delegator or an Admin (enforced in handler).
/// </summary>
[Route("api/v1/delegations")]
[Authorize]
public sealed class DelegationController(ISender mediator) : ApiControllerBase
{
    /// <summary>
    /// GET /api/v1/delegations
    /// List delegations filtered by delegatorId, delegateId, and/or status.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> List(
        [FromQuery] Guid? delegatorId,
        [FromQuery] Guid? delegateId,
        [FromQuery] bool? activeOnly,
        CancellationToken ct = default) =>
        ToApiResponse(await mediator.Send(new ListDelegationsQuery(delegatorId, delegateId, activeOnly), ct));

    /// <summary>
    /// POST /api/v1/delegations
    /// Create a new role delegation for a bounded time window.
    /// </summary>
    [HttpPost]
    [EnableRateLimiting("write-ops")]
    [ProducesResponseType(201)]
    [ProducesResponseType(409)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Create(
        [FromBody] DelegateRoleRequest request,
        CancellationToken ct = default)
    {
        var actorId = GetActorId();
        var result = await mediator.Send(
            new DelegateRoleCommand(
                request.DelegatorId,
                request.DelegateId,
                request.ValidFrom,
                request.ValidTo,
                request.Reason,
                actorId), ct);

        if (result.IsSuccess)
            return CreatedAtAction(nameof(List), new { delegatorId = request.DelegatorId }, new { id = result.Value });

        return ToApiResponse(result);
    }

    /// <summary>
    /// POST /api/v1/delegations/{id}/approve
    /// Approve a PendingApproval delegation. Admin/SystemAdmin only (enforced in handler).
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [EnableRateLimiting("write-ops")]
    [ProducesResponseType(204)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct = default) =>
        ToApiResponse(await mediator.Send(new ApproveDelegationCommand(id, GetActorId()), ct));

    /// <summary>
    /// POST /api/v1/delegations/{id}/revoke
    /// Revoke an active delegation via POST (alternative to DELETE).
    /// </summary>
    [HttpPost("{id:guid}/revoke")]
    [EnableRateLimiting("write-ops")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> RevokePost(Guid id, CancellationToken ct = default) =>
        ToApiResponse(await mediator.Send(new RevokeDelegationCommand(id, GetActorId()), ct));

    /// <summary>
    /// DELETE /api/v1/delegations/{id}
    /// Revoke an active delegation (REST-idiomatic).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [EnableRateLimiting("write-ops")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Revoke(Guid id, CancellationToken ct = default) =>
        ToApiResponse(await mediator.Send(new RevokeDelegationCommand(id, GetActorId()), ct));

    // --- helpers ---

    private Guid GetActorId()
    {
        var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}

/// <summary>Request body for delegation creation.</summary>
public sealed record DelegateRoleRequest(
    Guid DelegatorId,
    Guid DelegateId,
    DateTime ValidFrom,
    DateTime ValidTo,
    string Reason);
