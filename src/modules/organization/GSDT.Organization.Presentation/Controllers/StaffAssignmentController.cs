using MediatR;

namespace GSDT.Organization.Presentation.Controllers;

/// <summary>
/// Staff assignment and transfer endpoints — Admin only.
/// </summary>
[ApiController]
[Route("api/v1/admin/org/staff")]
[Authorize(Roles = "Admin,SystemAdmin")]
public class StaffAssignmentController(ISender mediator) : ApiControllerBase
{
    [HttpPost("{userId:guid}/assign")]
    [EnableRateLimiting("write-ops")]
    public async Task<IActionResult> Assign(
        Guid userId,
        [FromBody] AssignStaffRequest req, CancellationToken ct)
    {
        var tenantId = ResolveTenantId();
        return ToApiResponse(await mediator.Send(
            new AssignStaffCommand(tenantId, userId, req.OrgUnitId, req.RoleInOrg, req.PositionTitle), ct));
    }

    [HttpPost("{userId:guid}/transfer")]
    [EnableRateLimiting("write-ops")]
    public async Task<IActionResult> Transfer(
        Guid userId,
        [FromBody] TransferStaffRequest req, CancellationToken ct)
    {
        var tenantId = ResolveTenantId();
        return ToApiResponse(await mediator.Send(
            new TransferStaffCommand(tenantId, userId, req.ToOrgUnitId, req.RoleInOrg, req.PositionTitle), ct));
    }

    [HttpGet("{userId:guid}/history")]
    public async Task<IActionResult> History(
        Guid userId, CancellationToken ct)
    {
        var tenantId = ResolveTenantId();
        return ToApiResponse(await mediator.Send(new GetStaffHistoryQuery(userId, tenantId), ct));
    }
}

// --- Request DTOs ---

public sealed record AssignStaffRequest(Guid OrgUnitId, string RoleInOrg, string PositionTitle);
public sealed record TransferStaffRequest(Guid ToOrgUnitId, string RoleInOrg, string PositionTitle);
