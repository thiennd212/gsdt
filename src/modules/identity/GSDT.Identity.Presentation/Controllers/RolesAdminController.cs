using MediatR;

namespace GSDT.Identity.Presentation.Controllers;

/// <summary>
/// CRUD endpoints for the RBAC role catalogue.
/// All operations require Admin policy. System roles have restricted mutation rules.
/// </summary>
[Route("api/v1/admin/roles")]
[Authorize(Policy = "Admin")]
public sealed class RolesAdminController(ISender mediator) : ApiControllerBase
{
    /// <summary>Returns all role definitions with permission counts.</summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetRoles(CancellationToken ct = default) =>
        ToApiResponse(await mediator.Send(new GetRolesQuery(), ct));

    /// <summary>Returns full detail for a single role including assigned permissions.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default) =>
        ToApiResponse(await mediator.Send(new GetRoleByIdQuery(id), ct));

    /// <summary>Creates a new business role. Code must be unique.</summary>
    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Create([FromBody] CreateRoleCommand cmd, CancellationToken ct = default) =>
        ToApiResponse(await mediator.Send(cmd, ct));

    /// <summary>
    /// Updates Name and Description. System roles: Description only — Name is immutable.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleRequest body, CancellationToken ct = default) =>
        ToApiResponse(await mediator.Send(new UpdateRoleCommand(id, body.Name, body.Description), ct));

    /// <summary>Soft-deletes a business role (sets IsActive = false). System roles are rejected.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default) =>
        ToApiResponse(await mediator.Send(new DeleteRoleCommand(id), ct));
}

/// <summary>Request body for PUT /roles/{id} — keeps Id in route, body carries mutable fields only.</summary>
public sealed record UpdateRoleRequest(string Name, string? Description);
