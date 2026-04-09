namespace GSDT.InvestmentProjects.Presentation.Controllers;

/// <summary>
/// CRUD + sub-entity endpoints for FDI (foreign direct investment) projects.
/// Same as NĐT — no investor-selection, no design-estimate endpoints.
/// </summary>
[ApiController]
[Route("api/v1/fdi-projects")]
public sealed class FdiProjectsController(ISender sender) : ApiControllerBase
{
    // ── Core CRUD ─────────────────────────────────────────────────────────────

    [HttpGet]
    [Authorize(Roles = "Admin,BTC,CQCQ,CDT")]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
        [FromQuery] Guid? competentAuthorityId = null,
        [FromQuery] string? investorName = null,
        [FromQuery] Guid? statusId = null,
        [FromQuery] Guid? locationProvinceId = null)
        => ToApiResponse(await sender.Send(
            new ListFdiProjectsQuery(page, pageSize, search, competentAuthorityId,
                investorName, statusId, locationProvinceId)));

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,BTC,CQCQ,CDT")]
    public async Task<IActionResult> GetById(Guid id)
        => ToApiResponse(await sender.Send(new GetFdiProjectByIdQuery(id)));

    [HttpPost]
    [Authorize(Roles = "Admin,BTC,CDT")]
    public async Task<IActionResult> Create([FromBody] CreateFdiProjectCommand command)
        => ToApiResponse(await sender.Send(command));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,BTC,CDT")]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateFdiProjectCommand command)
    {
        if (id != command.Id) return BadRequest("Route id does not match body Id.");
        return ToApiResponse(await sender.Send(command));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,BTC,CDT")]
    public async Task<IActionResult> Delete(Guid id)
        => ToApiResponse(await sender.Send(new DeleteProjectCommand(id)));

    // ── Locations ─────────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/locations")]
    [Authorize(Roles = "Admin,BTC,CDT")]
    public async Task<IActionResult> AddLocation(
        Guid id, [FromBody] AddProjectLocationCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id }));

    [HttpDelete("{id:guid}/locations/{locationId:guid}")]
    [Authorize(Roles = "Admin,BTC,CDT")]
    public async Task<IActionResult> DeleteLocation(Guid id, Guid locationId)
        => ToApiResponse(await sender.Send(new DeleteProjectLocationCommand(id, locationId)));

    // ── Investment Decisions ──────────────────────────────────────────────────

    [HttpPost("{id:guid}/decisions")]
    [Authorize(Roles = "Admin,BTC,CDT")]
    public async Task<IActionResult> AddDecision(
        Guid id, [FromBody] AddFdiDecisionCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id }));

    [HttpDelete("{id:guid}/decisions/{decisionId:guid}")]
    [Authorize(Roles = "Admin,BTC,CDT")]
    public async Task<IActionResult> DeleteDecision(Guid id, Guid decisionId)
        => ToApiResponse(await sender.Send(new DeleteFdiDecisionCommand(id, decisionId)));

    // ── Registration Certificates ────────────────────────────────────────────

    [HttpPost("{id:guid}/certificates")]
    [Authorize(Roles = "Admin,BTC,CDT")]
    public async Task<IActionResult> AddCertificate(
        Guid id, [FromBody] AddRegistrationCertificateCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id }));

    [HttpPut("{id:guid}/certificates/{certificateId:guid}")]
    [Authorize(Roles = "Admin,BTC,CDT")]
    public async Task<IActionResult> UpdateCertificate(
        Guid id, Guid certificateId, [FromBody] UpdateRegistrationCertificateCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id, CertificateId = certificateId }));

    [HttpDelete("{id:guid}/certificates/{certificateId:guid}")]
    [Authorize(Roles = "Admin,BTC,CDT")]
    public async Task<IActionResult> DeleteCertificate(Guid id, Guid certificateId)
        => ToApiResponse(await sender.Send(new DeleteRegistrationCertificateCommand(id, certificateId)));

    // ── Bid Packages ──────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/bid-packages")]
    [Authorize(Roles = "Admin,BTC,CDT")]
    public async Task<IActionResult> AddBidPackage(
        Guid id, [FromBody] AddBidPackageCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id }));

    [HttpDelete("{id:guid}/bid-packages/{bidPackageId:guid}")]
    [Authorize(Roles = "Admin,BTC,CDT")]
    public async Task<IActionResult> DeleteBidPackage(Guid id, Guid bidPackageId)
        => ToApiResponse(await sender.Send(new DeleteBidPackageCommand(id, bidPackageId)));

    // ── Documents ─────────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/documents")]
    [Authorize(Roles = "Admin,BTC,CDT")]
    public async Task<IActionResult> AddDocument(
        Guid id, [FromBody] AddProjectDocumentCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id }));

    [HttpDelete("{id:guid}/documents/{documentId:guid}")]
    [Authorize(Roles = "Admin,BTC,CDT")]
    public async Task<IActionResult> DeleteDocument(Guid id, Guid documentId)
        => ToApiResponse(await sender.Send(new DeleteProjectDocumentCommand(id, documentId)));
}
