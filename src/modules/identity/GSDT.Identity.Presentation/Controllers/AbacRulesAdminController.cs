using MediatR;

namespace GSDT.Identity.Presentation.Controllers;

/// <summary>
/// Admin CRUD for ABAC attribute rules — department/classification access policies.
/// Requires Admin or SystemAdmin role.
/// </summary>
[Route("api/v1/admin/abac-rules")]
[Authorize(Policy = "Admin")]
[EnableRateLimiting("write-ops")]
public sealed class AbacRulesAdminController(ISender mediator) : ApiControllerBase
{
    /// <summary>List all ABAC rules for the resolved tenant.</summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> List(CancellationToken ct = default) =>
        ToApiResponse(await mediator.Send(new ListAbacRulesQuery(ResolveTenantId()), ct));

    /// <summary>Create a new ABAC rule.</summary>
    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAbacRuleRequest request,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new CreateAbacRuleCommand(
            request.Resource, request.Action, request.AttributeKey,
            request.AttributeValue, request.Effect, ResolveTenantId()), ct);

        if (result.IsSuccess)
            return StatusCode(201, new { id = result.Value });

        return ToApiResponse(result);
    }

    /// <summary>Update an ABAC rule.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] CreateAbacRuleRequest request,
        CancellationToken ct = default) =>
        ToApiResponse(await mediator.Send(new UpdateAbacRuleCommand(
            id, request.Resource, request.Action, request.AttributeKey,
            request.AttributeValue, request.Effect, ResolveTenantId()), ct));

    /// <summary>Delete an ABAC rule by ID.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default) =>
        ToApiResponse(await mediator.Send(new DeleteAbacRuleCommand(id), ct));
}

public sealed record CreateAbacRuleRequest(
    string Resource, string Action, string AttributeKey,
    string AttributeValue, string Effect);
