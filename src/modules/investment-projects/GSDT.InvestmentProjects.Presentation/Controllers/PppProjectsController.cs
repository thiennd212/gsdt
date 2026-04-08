namespace GSDT.InvestmentProjects.Presentation.Controllers;

/// <summary>
/// CRUD + sub-entity endpoints for PPP (Public-Private Partnership) investment projects.
/// All mutations resolve TenantId from JWT via ITenantContext (ambient — no explicit parameter).
/// Role access: BTC + CQCQ + CDT can read; BTC + CDT can write (ownership enforced in handlers).
/// </summary>
[ApiController]
[Route("api/v1/ppp-projects")]
public sealed class PppProjectsController(ISender sender) : ApiControllerBase
{
    // ── Core CRUD ─────────────────────────────────────────────────────────────

    [HttpGet]
    [Authorize(Roles = "BTC,CQCQ,CDT")]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
        [FromQuery] int? contractType = null,
        [FromQuery] Guid? competentAuthorityId = null,
        [FromQuery] Guid? statusId = null)
        => ToApiResponse(await sender.Send(
            new ListPppProjectsQuery(page, pageSize, search, contractType, competentAuthorityId, statusId)));

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "BTC,CQCQ,CDT")]
    public async Task<IActionResult> GetById(Guid id)
        => ToApiResponse(await sender.Send(new GetPppProjectByIdQuery(id)));

    [HttpPost]
    [Authorize(Roles = "BTC,CDT")]
    public async Task<IActionResult> Create([FromBody] CreatePppProjectCommand command)
        => ToApiResponse(await sender.Send(command));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "BTC,CDT")]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdatePppProjectCommand command)
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

    // ── Investment Decisions ──────────────────────────────────────────────────

    [HttpPost("{id:guid}/decisions")]
    [Authorize(Roles = "BTC,CDT")]
    public async Task<IActionResult> AddDecision(
        Guid id, [FromBody] AddPppDecisionCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id }));

    [HttpDelete("{id:guid}/decisions/{decisionId:guid}")]
    [Authorize(Roles = "BTC,CDT")]
    public async Task<IActionResult> DeleteDecision(Guid id, Guid decisionId)
        => ToApiResponse(await sender.Send(new DeletePppDecisionCommand(id, decisionId)));

    // ── Investor Selection ────────────────────────────────────────────────────

    [HttpPut("{id:guid}/investor-selection")]
    [Authorize(Roles = "BTC,CDT")]
    public async Task<IActionResult> UpsertInvestorSelection(
        Guid id, [FromBody] UpsertInvestorSelectionCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id }));

    // ── Contract Info ─────────────────────────────────────────────────────────

    [HttpPut("{id:guid}/contract-info")]
    [Authorize(Roles = "BTC,CDT")]
    public async Task<IActionResult> UpsertContractInfo(
        Guid id, [FromBody] UpsertPppContractInfoCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id }));

    // ── Capital Plans ─────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/capital-plans")]
    [Authorize(Roles = "BTC,CDT")]
    public async Task<IActionResult> AddCapitalPlan(
        Guid id, [FromBody] AddPppCapitalPlanCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id }));

    [HttpDelete("{id:guid}/capital-plans/{planId:guid}")]
    [Authorize(Roles = "BTC,CDT")]
    public async Task<IActionResult> DeleteCapitalPlan(Guid id, Guid planId)
        => ToApiResponse(await sender.Send(new DeletePppCapitalPlanCommand(id, planId)));

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

    // ── Design Estimates ──────────────────────────────────────────────────────

    [HttpPost("{id:guid}/design-estimates")]
    [Authorize(Roles = "BTC,CDT")]
    public async Task<IActionResult> AddDesignEstimate(
        Guid id, [FromBody] AddDesignEstimateCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id }));

    [HttpPut("{id:guid}/design-estimates/{estimateId:guid}")]
    [Authorize(Roles = "BTC,CDT")]
    public async Task<IActionResult> UpdateDesignEstimate(
        Guid id, Guid estimateId, [FromBody] UpdateDesignEstimateCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id, EstimateId = estimateId }));

    [HttpDelete("{id:guid}/design-estimates/{estimateId:guid}")]
    [Authorize(Roles = "BTC,CDT")]
    public async Task<IActionResult> DeleteDesignEstimate(Guid id, Guid estimateId)
        => ToApiResponse(await sender.Send(new DeleteDesignEstimateCommand(id, estimateId)));

    // ── Execution Records ─────────────────────────────────────────────────────

    [HttpPost("{id:guid}/execution-records")]
    [Authorize(Roles = "BTC,CDT")]
    public async Task<IActionResult> AddExecutionRecord(
        Guid id, [FromBody] AddPppExecutionCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id }));

    [HttpDelete("{id:guid}/execution-records/{recordId:guid}")]
    [Authorize(Roles = "BTC,CDT")]
    public async Task<IActionResult> DeleteExecutionRecord(Guid id, Guid recordId)
        => ToApiResponse(await sender.Send(new DeletePppExecutionCommand(id, recordId)));

    // ── Disbursements ─────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/disbursements")]
    [Authorize(Roles = "BTC,CDT")]
    public async Task<IActionResult> AddDisbursement(
        Guid id, [FromBody] AddPppDisbursementCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id }));

    [HttpDelete("{id:guid}/disbursements/{recordId:guid}")]
    [Authorize(Roles = "BTC,CDT")]
    public async Task<IActionResult> DeleteDisbursement(Guid id, Guid recordId)
        => ToApiResponse(await sender.Send(new DeletePppDisbursementCommand(id, recordId)));

    // ── Revenue Reports ───────────────────────────────────────────────────────

    [HttpPost("{id:guid}/revenue-reports")]
    [Authorize(Roles = "BTC,CDT")]
    public async Task<IActionResult> AddRevenueReport(
        Guid id, [FromBody] AddRevenueReportCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id }));

    [HttpPut("{id:guid}/revenue-reports/{reportId:guid}")]
    [Authorize(Roles = "BTC,CDT")]
    public async Task<IActionResult> UpdateRevenueReport(
        Guid id, Guid reportId, [FromBody] UpdateRevenueReportCommand command)
        => ToApiResponse(await sender.Send(command with { ProjectId = id, ReportId = reportId }));

    [HttpDelete("{id:guid}/revenue-reports/{reportId:guid}")]
    [Authorize(Roles = "BTC,CDT")]
    public async Task<IActionResult> DeleteRevenueReport(Guid id, Guid reportId)
        => ToApiResponse(await sender.Send(new DeleteRevenueReportCommand(id, reportId)));

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
