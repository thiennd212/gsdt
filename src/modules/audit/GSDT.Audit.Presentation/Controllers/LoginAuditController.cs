using MediatR;

namespace GSDT.Audit.Presentation.Controllers;

/// <summary>Login audit dashboard — suspicious authentication activity detection.</summary>
[Route("api/v1/admin/login-audit")]
[Authorize(Roles = "Admin,SystemAdmin,GovOfficer")]
public sealed class LoginAuditController(ISender mediator) : ApiControllerBase
{
    /// <summary>Query login attempts with filters.</summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? userId,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] bool? success,
        [FromQuery][StringLength(200)] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        return ToApiResponse(await mediator.Send(
            new GetLoginAuditQuery(userId, from, to, success, search, page, pageSize),
            cancellationToken));
    }
}
