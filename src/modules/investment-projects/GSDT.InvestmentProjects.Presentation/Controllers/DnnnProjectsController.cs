namespace GSDT.InvestmentProjects.Presentation.Controllers;

/// <summary>
/// CRUD + sub-entity endpoints for DNNN (state-owned enterprise) investment projects.
/// All mutations resolve TenantId from JWT via ITenantContext (ambient — no explicit parameter).
/// Role access: BTC + CQCQ + CDT can read; BTC + CDT can write (ownership enforced in handlers).
/// </summary>
[ApiController]
[Route("api/v1/dnnn-projects")]
public sealed class DnnnProjectsController(ISender sender) : ApiControllerBase
{
    // ── Core CRUD ─────────────────────────────────────────────────────────────

    [HttpGet]
    [RequirePermission(Permissions.Inv.DnnnRead)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
        [FromQuery] Guid? competentAuthorityId = null,
        [FromQuery] string? investorName = null,
        [FromQuery] Guid? statusId = null,
        [FromQuery] Guid? locationProvinceId = null)
        => ToApiResponse(await sender.Send(
            new ListDnnnProjectsQuery(page, pageSize, search, competentAuthorityId,
                investorName, statusId, locationProvinceId)));

    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.Inv.DnnnRead)]
    public async Task<IActionResult> GetById(Guid id)
        => ToApiResponse(await sender.Send(new GetDnnnProjectByIdQuery(id)));

    [HttpPost]
    [RequirePermission(Permissions.Inv.DnnnWrite)]
    public async Task<IActionResult> Create([FromBody] CreateDnnnProjectCommand command)
        => ToApiResponse(await sender.Send(command));

    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.Inv.DnnnWrite)]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateDnnnProjectCommand command)
    {
        if (id != command.Id) return BadRequest("Route id does not match body Id.");
        return ToApiResponse(await sender.Send(command));
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.Inv.DnnnDelete)]
    public async Task<IActionResult> Delete(Guid id)
        => ToApiResponse(await sender.Send(new DeleteProjectCommand(id)));

    // ── Locations ─────────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/locations")]
    [RequirePermission(Permissions.Inv.DnnnWrite)]
    public async Task<IActionResult> AddLocation(
        Guid id, [FromBody] AddProjectLocationCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id }));

    [HttpDelete("{id:guid}/locations/{locationId:guid}")]
    [RequirePermission(Permissions.Inv.DnnnDelete)]
    public async Task<IActionResult> DeleteLocation(Guid id, Guid locationId)
        => ToApiResponse(await sender.Send(new DeleteProjectLocationCommand(id, locationId)));

    // ── Investment Decisions ──────────────────────────────────────────────────

    [HttpPost("{id:guid}/decisions")]
    [RequirePermission(Permissions.Inv.DnnnWrite)]
    public async Task<IActionResult> AddDecision(
        Guid id, [FromBody] AddDnnnDecisionCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id }));

    [HttpDelete("{id:guid}/decisions/{decisionId:guid}")]
    [RequirePermission(Permissions.Inv.DnnnDelete)]
    public async Task<IActionResult> DeleteDecision(Guid id, Guid decisionId)
        => ToApiResponse(await sender.Send(new DeleteDnnnDecisionCommand(id, decisionId)));

    // ── Registration Certificates ────────────────────────────────────────────

    [HttpPost("{id:guid}/certificates")]
    [RequirePermission(Permissions.Inv.DnnnWrite)]
    public async Task<IActionResult> AddCertificate(
        Guid id, [FromBody] AddRegistrationCertificateCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id }));

    [HttpPut("{id:guid}/certificates/{certificateId:guid}")]
    [RequirePermission(Permissions.Inv.DnnnWrite)]
    public async Task<IActionResult> UpdateCertificate(
        Guid id, Guid certificateId, [FromBody] UpdateRegistrationCertificateCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id, CertificateId = certificateId }));

    [HttpDelete("{id:guid}/certificates/{certificateId:guid}")]
    [RequirePermission(Permissions.Inv.DnnnDelete)]
    public async Task<IActionResult> DeleteCertificate(Guid id, Guid certificateId)
        => ToApiResponse(await sender.Send(new DeleteRegistrationCertificateCommand(id, certificateId)));

    // ── Investor Selection ────────────────────────────────────────────────────

    [HttpPut("{id:guid}/investor-selection")]
    [RequirePermission(Permissions.Inv.DnnnWrite)]
    public async Task<IActionResult> UpsertInvestorSelection(
        Guid id, [FromBody] UpsertInvestorSelectionCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id }));

    // ── Design Estimates ──────────────────────────────────────────────────────

    [HttpPost("{id:guid}/design-estimates")]
    [RequirePermission(Permissions.Inv.DnnnWrite)]
    public async Task<IActionResult> AddDesignEstimate(
        Guid id, [FromBody] AddDesignEstimateCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id }));

    [HttpPut("{id:guid}/design-estimates/{estimateId:guid}")]
    [RequirePermission(Permissions.Inv.DnnnWrite)]
    public async Task<IActionResult> UpdateDesignEstimate(
        Guid id, Guid estimateId, [FromBody] UpdateDesignEstimateCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id, EstimateId = estimateId }));

    [HttpDelete("{id:guid}/design-estimates/{estimateId:guid}")]
    [RequirePermission(Permissions.Inv.DnnnDelete)]
    public async Task<IActionResult> DeleteDesignEstimate(Guid id, Guid estimateId)
        => ToApiResponse(await sender.Send(new DeleteDesignEstimateCommand(id, estimateId)));

    // ── Bid Packages ──────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/bid-packages")]
    [RequirePermission(Permissions.Inv.DnnnWrite)]
    public async Task<IActionResult> AddBidPackage(
        Guid id, [FromBody] AddBidPackageCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id }));

    [HttpDelete("{id:guid}/bid-packages/{bidPackageId:guid}")]
    [RequirePermission(Permissions.Inv.DnnnDelete)]
    public async Task<IActionResult> DeleteBidPackage(Guid id, Guid bidPackageId)
        => ToApiResponse(await sender.Send(new DeleteBidPackageCommand(id, bidPackageId)));

    // ── Documents ─────────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/documents")]
    [RequirePermission(Permissions.Inv.DnnnWrite)]
    public async Task<IActionResult> AddDocument(
        Guid id, [FromBody] AddProjectDocumentCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id }));

    [HttpDelete("{id:guid}/documents/{documentId:guid}")]
    [RequirePermission(Permissions.Inv.DnnnDelete)]
    public async Task<IActionResult> DeleteDocument(Guid id, Guid documentId)
        => ToApiResponse(await sender.Send(new DeleteProjectDocumentCommand(id, documentId)));
}
