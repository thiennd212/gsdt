using MediatR;

namespace GSDT.Identity.Presentation.Controllers;

/// <summary>
/// Read-only endpoint exposing the static RBAC role catalogue.
/// Used by the FE Roles page to display role definitions and their policies/permissions.
/// </summary>
[Route("api/v1/admin/roles")]
[Authorize(Policy = "Admin")]
public sealed class RolesAdminController(ISender mediator) : ApiControllerBase
{
    /// <summary>Returns all role definitions with their associated policies and permissions.</summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetRoles(CancellationToken ct = default) =>
        ToApiResponse(await mediator.Send(new GetRolesQuery(), ct));
}
