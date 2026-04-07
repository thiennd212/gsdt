using MediatR;

namespace GSDT.Organization.Controllers;

/// <summary>
/// Admin CRUD for org units + tree/member reads for authenticated users.
/// Writes require Admin role; reads require Authenticated only.
/// </summary>
[ApiController]
[Route("api/v1/admin/org/units")]
[Authorize]
public class OrgUnitsController(ISender mediator) : ApiControllerBase
{
    /// <summary>GET full flat tree — clients build hierarchy from ParentId.</summary>
    [HttpGet]
    public async Task<IActionResult> GetTree([FromQuery] Guid tenantId, CancellationToken ct)
        => ToApiResponse(await mediator.Send(new GetOrgTreeQuery(tenantId), ct));

    /// <summary>GET single org unit by Id.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid tenantId, CancellationToken ct)
        => ToApiResponse(await mediator.Send(new GetOrgUnitQuery(id, tenantId), ct));

    /// <summary>GET active staff members of an org unit.</summary>
    [HttpGet("{id:guid}/members")]
    public async Task<IActionResult> GetMembers(Guid id, [FromQuery] Guid tenantId, CancellationToken ct)
        => ToApiResponse(await mediator.Send(new GetOrgUnitMembersQuery(id, tenantId), ct));

    /// <summary>POST create org unit — Admin only.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,SystemAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateOrgUnitRequest req, [FromQuery] Guid tenantId, CancellationToken ct)
        => ToApiResponse(await mediator.Send(
            new CreateOrgUnitCommand(tenantId, req.Name, req.NameEn, req.Code, req.ParentId), ct));

    /// <summary>PUT update name fields — Admin only.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,SystemAdmin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrgUnitRequest req, [FromQuery] Guid tenantId, CancellationToken ct)
        => ToApiResponse(await mediator.Send(
            new UpdateOrgUnitCommand(id, tenantId, req.Name, req.NameEn), ct));

    /// <summary>DELETE (soft-deactivate) — Admin only. Blocked if active children exist.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,SystemAdmin")]
    public async Task<IActionResult> Delete(Guid id, [FromQuery] Guid tenantId, [FromQuery] Guid? successorId, CancellationToken ct)
        => ToApiResponse(await mediator.Send(
            new DeleteOrgUnitCommand(id, tenantId, successorId), ct));
}
