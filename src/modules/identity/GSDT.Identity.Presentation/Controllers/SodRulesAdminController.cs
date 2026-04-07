using MediatR;

namespace GSDT.Identity.Presentation.Controllers;

/// <summary>
/// Admin CRUD for Segregation of Duties conflict rules.
/// Defines permission-code pairs that must not coexist on the same user.
/// Requires Admin or SystemAdmin role.
/// </summary>
[Route("api/v1/admin/sod-rules")]
[Authorize(Policy = "Admin")]
[EnableRateLimiting("write-ops")]
public sealed class SodRulesAdminController(ISender mediator) : ApiControllerBase
{
    /// <summary>List all SoD rules for the resolved tenant.</summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> List(CancellationToken ct = default) =>
        ToApiResponse(await mediator.Send(new ListSodRulesQuery(ResolveTenantId()), ct));

    /// <summary>Create a new SoD conflict rule.</summary>
    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(409)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSodRuleRequest request,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new CreateSodRuleCommand(
            request.PermissionCodeA,
            request.PermissionCodeB,
            request.EnforcementLevel,
            request.Description,
            ResolveTenantId()), ct);

        if (result.IsSuccess)
            return StatusCode(201, new { id = result.Value });

        return ToApiResponse(result);
    }

    /// <summary>Update an existing SoD conflict rule.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateSodRuleRequest request,
        CancellationToken ct = default) =>
        ToApiResponse(await mediator.Send(new UpdateSodRuleCommand(
            id,
            request.PermissionCodeA,
            request.PermissionCodeB,
            request.EnforcementLevel,
            request.Description,
            request.IsActive), ct));

    /// <summary>Delete a SoD conflict rule by ID.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default) =>
        ToApiResponse(await mediator.Send(new DeleteSodRuleCommand(id), ct));
}

public sealed record CreateSodRuleRequest(
    string PermissionCodeA,
    string PermissionCodeB,
    string EnforcementLevel,
    string? Description);

public sealed record UpdateSodRuleRequest(
    string PermissionCodeA,
    string PermissionCodeB,
    string EnforcementLevel,
    string? Description,
    bool IsActive);
