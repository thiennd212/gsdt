using MediatR;

namespace GSDT.MasterData.Controllers;

/// <summary>
/// External mapping CRUD endpoints — maps internal codes to external system codes.
/// Requires Admin role for writes; any authenticated user can read.
/// </summary>
[ApiController]
[Route("api/v1/external-mappings")]
[Authorize]
public sealed class ExternalMappingController(ISender mediator) : ApiControllerBase
{
    /// <summary>List external mappings with optional filters (paginated).</summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> List(
        [FromQuery] Guid tenantId,
        [FromQuery] string? externalSystem = null,
        [FromQuery] string? internalCode = null,
        [FromQuery] bool activeOnly = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        return ToApiResponse(await mediator.Send(
            new GetExternalMappingsQuery(tenantId, externalSystem, internalCode, activeOnly, page, pageSize), ct));
    }

    /// <summary>Create a new external mapping.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,SystemAdmin")]
    [ProducesResponseType(201)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Create(
        [FromBody] CreateExternalMappingRequest request,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new CreateExternalMappingCommand(
                request.InternalCode, request.ExternalSystem, request.ExternalCode,
                request.Direction, request.DictionaryId,
                request.ValidFrom, request.ValidTo,
                request.TenantId, GetActorId()), ct);

        if (result.IsSuccess)
            return CreatedAtAction(nameof(List), new { tenantId = request.TenantId }, new { id = result.Value });

        return ToApiResponse(result);
    }

    private Guid GetActorId()
    {
        var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}

// --- Request DTOs ---

public sealed record CreateExternalMappingRequest(
    string InternalCode,
    string ExternalSystem,
    string ExternalCode,
    MappingDirection Direction,
    Guid? DictionaryId,
    DateTime ValidFrom,
    DateTime? ValidTo,
    Guid TenantId);
