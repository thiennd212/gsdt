using MediatR;

namespace GSDT.Organization.Controllers;

/// <summary>
/// Staff assignment and transfer endpoints — Admin only.
/// All operations require tenantId as query param (consistent with other modules).
/// </summary>
[ApiController]
[Route("api/v1/admin/org/staff")]
[Authorize(Roles = "Admin,SystemAdmin")]
public class StaffAssignmentController(ISender mediator) : ApiControllerBase
{
    /// <summary>POST assign user to an org unit with role and position title.</summary>
    [HttpPost("{userId:guid}/assign")]
    public async Task<IActionResult> Assign(
        Guid userId,
        [FromQuery] Guid tenantId,
        [FromBody] AssignStaffRequest req,
        CancellationToken ct)
        => ToApiResponse(await mediator.Send(
            new AssignStaffCommand(tenantId, userId, req.OrgUnitId, req.RoleInOrg, req.PositionTitle), ct));

    /// <summary>POST transfer user to new org unit — closes old assignment, opens new one.</summary>
    [HttpPost("{userId:guid}/transfer")]
    public async Task<IActionResult> Transfer(
        Guid userId,
        [FromQuery] Guid tenantId,
        [FromBody] TransferStaffRequest req,
        CancellationToken ct)
        => ToApiResponse(await mediator.Send(
            new TransferStaffCommand(tenantId, userId, req.ToOrgUnitId, req.RoleInOrg, req.PositionTitle), ct));

    /// <summary>GET full assignment and position history for a user.</summary>
    [HttpGet("{userId:guid}/history")]
    public async Task<IActionResult> History(
        Guid userId,
        [FromQuery] Guid tenantId,
        CancellationToken ct)
        => ToApiResponse(await mediator.Send(new GetStaffHistoryQuery(userId, tenantId), ct));
}
