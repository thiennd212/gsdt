using MediatR;

namespace GSDT.Identity.Presentation.Controllers;

/// <summary>
/// External identity federation endpoints — link users to SSO/LDAP/VNeID/OAuth/SAML providers.
/// Requires Admin role.
/// </summary>
[Route("api/v1/admin/external-identities")]
[Authorize(Policy = "Admin")]
[EnableRateLimiting("write-ops")]
public sealed class ExternalIdentitiesController(ISender mediator) : ApiControllerBase
{
    /// <summary>List external identities for a user (paginated).</summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> List(
        [FromQuery] Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        return ToApiResponse(await mediator.Send(
            new ListExternalIdentitiesQuery(userId, page, pageSize), ct));
    }

    /// <summary>Get a single external identity by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default) =>
        ToApiResponse(await mediator.Send(new GetExternalIdentityByIdQuery(id), ct));

    /// <summary>Link a new external identity to a user.</summary>
    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(409)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Create(
        [FromBody] CreateExternalIdentityRequest request,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new CreateExternalIdentityCommand(
                request.UserId, request.Provider, request.ExternalId,
                request.DisplayName, request.Email, GetActorId()), ct);

        if (result.IsSuccess)
            return CreatedAtAction(nameof(GetById), new { id = result.Value }, null);

        return ToApiResponse(result);
    }

    /// <summary>Sync/update display name, email or metadata on an existing external identity link.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateExternalIdentityRequest request,
        CancellationToken ct = default) =>
        ToApiResponse(await mediator.Send(
            new UpdateExternalIdentityCommand(
                id, request.DisplayName, request.Email, request.Metadata, GetActorId()), ct));

    /// <summary>Remove (soft-delete) an external identity link.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default) =>
        ToApiResponse(await mediator.Send(new DeleteExternalIdentityCommand(id, GetActorId()), ct));

    private Guid GetActorId()
    {
        var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}

public sealed record CreateExternalIdentityRequest(
    Guid UserId,
    ExternalIdentityProvider Provider,
    string ExternalId,
    string? DisplayName,
    string? Email);

public sealed record UpdateExternalIdentityRequest(
    string? DisplayName,
    string? Email,
    string? Metadata);
