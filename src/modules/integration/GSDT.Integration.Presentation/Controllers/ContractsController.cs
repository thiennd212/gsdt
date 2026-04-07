using MediatR;

namespace GSDT.Integration.Presentation.Controllers;

/// <summary>
/// Integration Contract REST API — manage data-scope contracts for partner relationships.
/// TenantId resolved from JWT claim (ITenantContext) — never caller-supplied.
/// </summary>
[Route("api/v1/integration/contracts")]
[Authorize]
public sealed class ContractsController(ISender mediator, ITenantContext tenantContext)
    : ApiControllerBase
{
    /// <summary>List contracts, optionally filtered by partnerId. pageSize clamped to [1, 100].</summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> List(
        [FromQuery] Guid? partnerId,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var tenantId = tenantContext.TenantId ?? Guid.Empty;
        pageSize = Math.Clamp(pageSize, 1, 100);
        return ToApiResponse(await mediator.Send(
            new ListContractsQuery(tenantId, partnerId, search, page, pageSize), ct));
    }

    /// <summary>Get contract detail by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var tenantId = tenantContext.TenantId ?? Guid.Empty;
        return ToApiResponse(await mediator.Send(
            new GetContractQuery(id, tenantId), ct));
    }

    /// <summary>Create a new integration contract (starts in Draft status).</summary>
    [HttpPost]
    [EnableRateLimiting("write-ops")]
    [ProducesResponseType(200)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Create(
        [FromBody] CreateContractBody body, CancellationToken ct)
    {
        var tenantId = tenantContext.TenantId ?? Guid.Empty;
        return ToApiResponse(await mediator.Send(
            new CreateContractCommand(tenantId, body.PartnerId, body.Title,
                body.Description, body.EffectiveDate, body.ExpiryDate,
                body.DataScopeJson), ct));
    }

    /// <summary>Update contract mutable fields (allowed in Draft or Active states).</summary>
    [HttpPut("{id:guid}")]
    [EnableRateLimiting("write-ops")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateContractBody body, CancellationToken ct) =>
        ToApiResponse(await mediator.Send(
            new UpdateContractCommand(id, body.Title, body.Description,
                body.EffectiveDate, body.ExpiryDate, body.DataScopeJson), ct));

    /// <summary>Soft-delete a contract.</summary>
    [HttpDelete("{id:guid}")]
    [EnableRateLimiting("write-ops")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        ToApiResponse(await mediator.Send(new DeleteContractCommand(id), ct));
}

// Request body records — lightweight, no entity types (GOV_SEC_004 mass-assignment prevention)
public sealed record CreateContractBody(
    Guid PartnerId, string Title, string? Description,
    DateTime EffectiveDate, DateTime? ExpiryDate, string? DataScopeJson);

public sealed record UpdateContractBody(
    string Title, string? Description,
    DateTime EffectiveDate, DateTime? ExpiryDate, string? DataScopeJson);
