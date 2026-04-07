using MediatR;

namespace GSDT.Organization.Presentation.Controllers;

/// <summary>
/// Admin CRUD for org units + tree/member reads for authenticated users.
/// Writes require Admin role; reads require Authenticated only.
/// </summary>
[ApiController]
[Route("api/v1/admin/org/units")]
[Authorize]
public class OrgUnitsController(ISender mediator) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTree(CancellationToken ct)
    {
        var tenantId = ResolveTenantId();
        return ToApiResponse(await mediator.Send(new GetOrgTreeQuery(tenantId), ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var tenantId = ResolveTenantId();
        return ToApiResponse(await mediator.Send(new GetOrgUnitQuery(id, tenantId), ct));
    }

    [HttpGet("{id:guid}/members")]
    public async Task<IActionResult> GetMembers(Guid id, CancellationToken ct)
    {
        var tenantId = ResolveTenantId();
        return ToApiResponse(await mediator.Send(new GetOrgUnitMembersQuery(id, tenantId), ct));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SystemAdmin")]
    [EnableRateLimiting("write-ops")]
    public async Task<IActionResult> Create([FromBody] CreateOrgUnitRequest req, CancellationToken ct)
    {
        var tenantId = ResolveTenantId();
        return ToApiResponse(await mediator.Send(
            new CreateOrgUnitCommand(tenantId, req.Name, req.NameEn, req.Code, req.ParentId), ct));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,SystemAdmin")]
    [EnableRateLimiting("write-ops")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrgUnitRequest req, CancellationToken ct)
    {
        var tenantId = ResolveTenantId();
        return ToApiResponse(await mediator.Send(
            new UpdateOrgUnitCommand(id, tenantId, req.Name, req.NameEn), ct));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,SystemAdmin")]
    [EnableRateLimiting("write-ops")]
    public async Task<IActionResult> Delete(Guid id, [FromQuery] Guid? successorId, CancellationToken ct)
    {
        var tenantId = ResolveTenantId();
        return ToApiResponse(await mediator.Send(
            new DeleteOrgUnitCommand(id, tenantId, successorId), ct));
    }
}

// --- Request DTOs ---

public sealed record CreateOrgUnitRequest(string Name, string NameEn, string Code, Guid? ParentId);
public sealed record UpdateOrgUnitRequest(string Name, string NameEn);
