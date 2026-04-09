using MediatR;
using GSDT.Identity.Application.Queries.GetPermissions;
using GSDT.Identity.Application.Queries.GetPermissionsByModule;

namespace GSDT.Identity.Presentation.Controllers;

/// <summary>
/// Read-only admin endpoints for the permission catalogue.
/// Permissions are seeded from code — mutations are not exposed here.
/// All endpoints require Admin policy.
/// </summary>
[Route("api/v1/admin/permissions")]
[Authorize(Policy = "Admin")]
public sealed class PermissionsAdminController(ISender mediator) : ApiControllerBase
{
    /// <summary>Returns all permissions, optionally filtered by module code or search term.</summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> List(
        [FromQuery] string? moduleCode,
        [FromQuery] string? search,
        CancellationToken ct = default)
        => ToApiResponse(await mediator.Send(new GetPermissionsQuery(moduleCode, search), ct));

    /// <summary>Returns all permissions grouped by module code.</summary>
    [HttpGet("by-module")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ListByModule(CancellationToken ct = default)
        => ToApiResponse(await mediator.Send(new GetPermissionsByModuleQuery(), ct));
}
