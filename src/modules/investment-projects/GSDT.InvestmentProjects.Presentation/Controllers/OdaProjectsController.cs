namespace GSDT.InvestmentProjects.Presentation.Controllers;

/// <summary>
/// CRUD + sub-entity endpoints for ODA-funded investment projects.
/// All mutations resolve TenantId from JWT via ITenantContext (ambient — no explicit parameter).
/// Role access: BTC + CQCQ + CDT can read; BTC + CDT can write (ownership enforced in handlers).
/// </summary>
[ApiController]
[Route("api/v1/oda-projects")]
public sealed class OdaProjectsController(ISender sender) : ApiControllerBase
{
    // ── Core CRUD ─────────────────────────────────────────────────────────────

    [HttpGet]
    [Authorize(Roles = "BTC,CQCQ,CDT")]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null)
        => ToApiResponse(await sender.Send(
            new ListOdaProjectsQuery(page, pageSize, search)));

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "BTC,CQCQ,CDT")]
    public async Task<IActionResult> GetById(Guid id)
        => ToApiResponse(await sender.Send(new GetOdaProjectByIdQuery(id)));

    [HttpPost]
    [Authorize(Roles = "BTC,CDT")]
    public async Task<IActionResult> Create([FromBody] CreateOdaProjectCommand command)
        => ToApiResponse(await sender.Send(command));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "BTC,CDT")]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateOdaProjectCommand command)
    {
        if (id != command.Id) return BadRequest("Route id does not match body Id.");
        return ToApiResponse(await sender.Send(command));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "BTC,CDT")]
    public async Task<IActionResult> Delete(Guid id)
        => ToApiResponse(await sender.Send(new DeleteProjectCommand(id)));

    // ── Locations ─────────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/locations")]
    [Authorize(Roles = "BTC,CDT")]
    public async Task<IActionResult> AddLocation(
        Guid id, [FromBody] AddProjectLocationCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id }));

    [HttpDelete("{id:guid}/locations/{locationId:guid}")]
    [Authorize(Roles = "BTC,CDT")]
    public async Task<IActionResult> DeleteLocation(Guid id, Guid locationId)
        => ToApiResponse(await sender.Send(new DeleteProjectLocationCommand(id, locationId)));

    // ── Bid Packages ──────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/bid-packages")]
    [Authorize(Roles = "BTC,CDT")]
    public async Task<IActionResult> AddBidPackage(
        Guid id, [FromBody] AddBidPackageCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id }));

    [HttpDelete("{id:guid}/bid-packages/{bidPackageId:guid}")]
    [Authorize(Roles = "BTC,CDT")]
    public async Task<IActionResult> DeleteBidPackage(Guid id, Guid bidPackageId)
        => ToApiResponse(await sender.Send(new DeleteBidPackageCommand(id, bidPackageId)));

    // ── Documents ─────────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/documents")]
    [Authorize(Roles = "BTC,CDT")]
    public async Task<IActionResult> AddDocument(
        Guid id, [FromBody] AddProjectDocumentCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id }));

    [HttpDelete("{id:guid}/documents/{documentId:guid}")]
    [Authorize(Roles = "BTC,CDT")]
    public async Task<IActionResult> DeleteDocument(Guid id, Guid documentId)
        => ToApiResponse(await sender.Send(new DeleteProjectDocumentCommand(id, documentId)));
}
