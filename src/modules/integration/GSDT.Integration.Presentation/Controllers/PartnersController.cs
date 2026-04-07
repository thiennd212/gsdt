using MediatR;

namespace GSDT.Integration.Presentation.Controllers;

/// <summary>
/// Integration Partner REST API — CRUD + lifecycle state transitions.
/// TenantId resolved from JWT claim (ITenantContext) — never caller-supplied.
/// </summary>
[Route("api/v1/integration/partners")]
[Authorize]
public sealed class PartnersController(ISender mediator, ITenantContext tenantContext)
    : ApiControllerBase
{
    /// <summary>List partners (paginated). pageSize clamped to [1, 100].</summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> List(
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var tenantId = tenantContext.TenantId ?? Guid.Empty;
        pageSize = Math.Clamp(pageSize, 1, 100);
        return ToApiResponse(await mediator.Send(
            new ListPartnersQuery(tenantId, search, page, pageSize), ct));
    }

    /// <summary>Get partner detail by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var tenantId = tenantContext.TenantId ?? Guid.Empty;
        return ToApiResponse(await mediator.Send(
            new GetPartnerQuery(id, tenantId), ct));
    }

    /// <summary>Create a new integration partner.</summary>
    [HttpPost]
    [EnableRateLimiting("write-ops")]
    [ProducesResponseType(200)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePartnerBody body, CancellationToken ct)
    {
        var tenantId = tenantContext.TenantId ?? Guid.Empty;
        return ToApiResponse(await mediator.Send(
            new CreatePartnerCommand(tenantId, body.Name, body.Code,
                body.ContactEmail, body.ContactPhone,
                body.Endpoint, body.AuthScheme), ct));
    }

    /// <summary>Update partner mutable fields.</summary>
    [HttpPut("{id:guid}")]
    [EnableRateLimiting("write-ops")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdatePartnerBody body, CancellationToken ct) =>
        ToApiResponse(await mediator.Send(
            new UpdatePartnerCommand(id, body.Name, body.Code,
                body.ContactEmail, body.ContactPhone,
                body.Endpoint, body.AuthScheme), ct));

    /// <summary>Soft-delete a partner.</summary>
    [HttpDelete("{id:guid}")]
    [EnableRateLimiting("write-ops")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        ToApiResponse(await mediator.Send(new DeletePartnerCommand(id), ct));
}

// Request body records — lightweight, no entity types (GOV_SEC_004 mass-assignment prevention)
public sealed record CreatePartnerBody(
    string Name, string Code, string? ContactEmail, string? ContactPhone,
    string? Endpoint, string? AuthScheme);

public sealed record UpdatePartnerBody(
    string Name, string Code, string? ContactEmail, string? ContactPhone,
    string? Endpoint, string? AuthScheme);
