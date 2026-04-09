namespace GSDT.InvestmentProjects.Presentation.Controllers;

/// <summary>
/// CRUD + sub-entity endpoints for domestic public-investment projects.
/// All mutations resolve TenantId from JWT via ITenantContext (ambient — no explicit parameter).
/// Role access: BTC + CQCQ + CDT can read; BTC + CDT can write (ownership enforced in handlers).
/// </summary>
[ApiController]
[Route("api/v1/domestic-projects")]
public sealed class DomesticProjectsController(ISender sender) : ApiControllerBase
{
    // ── Core CRUD ─────────────────────────────────────────────────────────────

    [HttpGet]
    [RequirePermission(Permissions.Inv.DomesticRead)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null)
        => ToApiResponse(await sender.Send(
            new ListDomesticProjectsQuery(page, pageSize, search)));

    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.Inv.DomesticRead)]
    public async Task<IActionResult> GetById(Guid id)
        => ToApiResponse(await sender.Send(new GetDomesticProjectByIdQuery(id)));

    [HttpPost]
    [RequirePermission(Permissions.Inv.DomesticWrite)]
    public async Task<IActionResult> Create([FromBody] CreateDomesticProjectCommand command)
        => ToApiResponse(await sender.Send(command));

    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.Inv.DomesticWrite)]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateDomesticProjectCommand command)
    {
        if (id != command.Id) return BadRequest("Route id does not match body Id.");
        return ToApiResponse(await sender.Send(command));
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.Inv.DomesticDelete)]
    public async Task<IActionResult> Delete(Guid id)
        => ToApiResponse(await sender.Send(new DeleteProjectCommand(id)));

    // ── Locations ─────────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/locations")]
    [RequirePermission(Permissions.Inv.DomesticWrite)]
    public async Task<IActionResult> AddLocation(
        Guid id, [FromBody] AddProjectLocationCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id }));

    [HttpDelete("{id:guid}/locations/{locationId:guid}")]
    [RequirePermission(Permissions.Inv.DomesticDelete)]
    public async Task<IActionResult> DeleteLocation(Guid id, Guid locationId)
        => ToApiResponse(await sender.Send(new DeleteProjectLocationCommand(id, locationId)));

    // ── Investment Decisions ───────────────────────────────────────────────────

    [HttpPost("{id:guid}/decisions")]
    [RequirePermission(Permissions.Inv.DomesticWrite)]
    public async Task<IActionResult> AddDecision(
        Guid id, [FromBody] AddDomesticDecisionCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id }));

    [HttpDelete("{id:guid}/decisions/{decisionId:guid}")]
    [RequirePermission(Permissions.Inv.DomesticDelete)]
    public async Task<IActionResult> DeleteDecision(Guid id, Guid decisionId)
        => ToApiResponse(await sender.Send(new DeleteDomesticDecisionCommand(id, decisionId)));

    // ── Bid Packages ──────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/bid-packages")]
    [RequirePermission(Permissions.Inv.DomesticWrite)]
    public async Task<IActionResult> AddBidPackage(
        Guid id, [FromBody] AddBidPackageCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id }));

    [HttpDelete("{id:guid}/bid-packages/{bidPackageId:guid}")]
    [RequirePermission(Permissions.Inv.DomesticDelete)]
    public async Task<IActionResult> DeleteBidPackage(Guid id, Guid bidPackageId)
        => ToApiResponse(await sender.Send(new DeleteBidPackageCommand(id, bidPackageId)));

    // ── Documents ─────────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/documents")]
    [RequirePermission(Permissions.Inv.DomesticWrite)]
    public async Task<IActionResult> AddDocument(
        Guid id, [FromBody] AddProjectDocumentCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id }));

    [HttpDelete("{id:guid}/documents/{documentId:guid}")]
    [RequirePermission(Permissions.Inv.DomesticDelete)]
    public async Task<IActionResult> DeleteDocument(Guid id, Guid documentId)
        => ToApiResponse(await sender.Send(new DeleteProjectDocumentCommand(id, documentId)));
}
