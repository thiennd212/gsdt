namespace GSDT.InvestmentProjects.Presentation.Controllers;

/// <summary>
/// CRUD + sub-entity endpoints for NĐT (private domestic investor) investment projects.
/// Same as DNNN minus investor-selection and design-estimate endpoints.
/// </summary>
[ApiController]
[Route("api/v1/ndt-projects")]
public sealed class NdtProjectsController(ISender sender) : ApiControllerBase
{
    // ── Core CRUD ─────────────────────────────────────────────────────────────

    [HttpGet]
    [RequirePermission(Permissions.Inv.NdtRead)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
        [FromQuery] Guid? competentAuthorityId = null,
        [FromQuery] string? investorName = null,
        [FromQuery] Guid? statusId = null,
        [FromQuery] Guid? locationProvinceId = null)
        => ToApiResponse(await sender.Send(
            new ListNdtProjectsQuery(page, pageSize, search, competentAuthorityId,
                investorName, statusId, locationProvinceId)));

    [HttpGet("{id:guid}")]
    [RequirePermission(Permissions.Inv.NdtRead)]
    public async Task<IActionResult> GetById(Guid id)
        => ToApiResponse(await sender.Send(new GetNdtProjectByIdQuery(id)));

    [HttpPost]
    [RequirePermission(Permissions.Inv.NdtWrite)]
    public async Task<IActionResult> Create([FromBody] CreateNdtProjectCommand command)
        => ToApiResponse(await sender.Send(command));

    [HttpPut("{id:guid}")]
    [RequirePermission(Permissions.Inv.NdtWrite)]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateNdtProjectCommand command)
    {
        if (id != command.Id) return BadRequest("Route id does not match body Id.");
        return ToApiResponse(await sender.Send(command));
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(Permissions.Inv.NdtDelete)]
    public async Task<IActionResult> Delete(Guid id)
        => ToApiResponse(await sender.Send(new DeleteProjectCommand(id)));

    // ── Locations ─────────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/locations")]
    [RequirePermission(Permissions.Inv.NdtWrite)]
    public async Task<IActionResult> AddLocation(
        Guid id, [FromBody] AddProjectLocationCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id }));

    [HttpDelete("{id:guid}/locations/{locationId:guid}")]
    [RequirePermission(Permissions.Inv.NdtDelete)]
    public async Task<IActionResult> DeleteLocation(Guid id, Guid locationId)
        => ToApiResponse(await sender.Send(new DeleteProjectLocationCommand(id, locationId)));

    // ── Investment Decisions ──────────────────────────────────────────────────

    [HttpPost("{id:guid}/decisions")]
    [RequirePermission(Permissions.Inv.NdtWrite)]
    public async Task<IActionResult> AddDecision(
        Guid id, [FromBody] AddNdtDecisionCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id }));

    [HttpDelete("{id:guid}/decisions/{decisionId:guid}")]
    [RequirePermission(Permissions.Inv.NdtDelete)]
    public async Task<IActionResult> DeleteDecision(Guid id, Guid decisionId)
        => ToApiResponse(await sender.Send(new DeleteNdtDecisionCommand(id, decisionId)));

    // ── Registration Certificates ────────────────────────────────────────────

    [HttpPost("{id:guid}/certificates")]
    [RequirePermission(Permissions.Inv.NdtWrite)]
    public async Task<IActionResult> AddCertificate(
        Guid id, [FromBody] AddRegistrationCertificateCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id }));

    [HttpPut("{id:guid}/certificates/{certificateId:guid}")]
    [RequirePermission(Permissions.Inv.NdtWrite)]
    public async Task<IActionResult> UpdateCertificate(
        Guid id, Guid certificateId, [FromBody] UpdateRegistrationCertificateCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id, CertificateId = certificateId }));

    [HttpDelete("{id:guid}/certificates/{certificateId:guid}")]
    [RequirePermission(Permissions.Inv.NdtDelete)]
    public async Task<IActionResult> DeleteCertificate(Guid id, Guid certificateId)
        => ToApiResponse(await sender.Send(new DeleteRegistrationCertificateCommand(id, certificateId)));

    // ── Bid Packages ──────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/bid-packages")]
    [RequirePermission(Permissions.Inv.NdtWrite)]
    public async Task<IActionResult> AddBidPackage(
        Guid id, [FromBody] AddBidPackageCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id }));

    [HttpDelete("{id:guid}/bid-packages/{bidPackageId:guid}")]
    [RequirePermission(Permissions.Inv.NdtDelete)]
    public async Task<IActionResult> DeleteBidPackage(Guid id, Guid bidPackageId)
        => ToApiResponse(await sender.Send(new DeleteBidPackageCommand(id, bidPackageId)));

    // ── Documents ─────────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/documents")]
    [RequirePermission(Permissions.Inv.NdtWrite)]
    public async Task<IActionResult> AddDocument(
        Guid id, [FromBody] AddProjectDocumentCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id }));

    [HttpDelete("{id:guid}/documents/{documentId:guid}")]
    [RequirePermission(Permissions.Inv.NdtDelete)]
    public async Task<IActionResult> DeleteDocument(Guid id, Guid documentId)
        => ToApiResponse(await sender.Send(new DeleteProjectDocumentCommand(id, documentId)));
}
