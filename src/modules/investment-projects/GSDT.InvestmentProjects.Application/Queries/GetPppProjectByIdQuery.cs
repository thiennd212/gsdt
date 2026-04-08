namespace GSDT.InvestmentProjects.Application.Queries;

/// <summary>
/// Returns full detail of a single PPP project with all child collections.
/// Delegates eager-loading to IInvestmentProjectRepository to avoid Application → Infrastructure dependency.
/// </summary>
public sealed record GetPppProjectByIdQuery(Guid Id)
    : IRequest<Result<PppProjectDetailDto>>;

public sealed class GetPppProjectByIdQueryHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext)
    : IRequestHandler<GetPppProjectByIdQuery, Result<PppProjectDetailDto>>
{
    public async Task<Result<PppProjectDetailDto>> Handle(
        GetPppProjectByIdQuery request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail<PppProjectDetailDto>("GOV_INV_401: Tenant context is required.");

        var project = await repository.GetPppByIdWithDetailsAsync(
            request.Id, tenantId, cancellationToken);

        if (project is null)
            return Result.Fail<PppProjectDetailDto>(
                $"GOV_INV_404: Du an PPP khong ton tai (Id={request.Id}).");

        return Result.Ok(MapToDetailDto(project));
    }

    private static PppProjectDetailDto MapToDetailDto(PppProject p) =>
        new(
            p.Id, p.ProjectCode, p.ProjectName,
            (int)p.ContractType, (int)p.SubProjectType,
            p.ProjectGroupId, p.StatusId,
            p.ManagingAuthorityId, p.IndustrySectorId, p.ProjectOwnerId,
            p.CompetentAuthorityId, p.PreparationUnit, p.Objective,
            p.ProjectManagementUnitId, p.PmuDirectorName, p.PmuPhone, p.PmuEmail,
            p.ImplementationPeriod,
            p.PolicyDecisionNumber, p.PolicyDecisionDate,
            p.PolicyDecisionAuthority, p.PolicyDecisionPerson, p.PolicyDecisionFileId,
            p.PrelimTotalInvestment, p.PrelimStateCapital,
            p.PrelimEquityCapital, p.PrelimLoanCapital,
            p.AreaHectares, p.Capacity, p.MainItems,
            p.StopContent, p.StopDecisionNumber, p.StopDecisionDate, p.StopFileId,
            // Shared child collections
            p.Locations.Select(l => new ProjectLocationDto(
                l.Id, l.ProvinceId, l.DistrictId, l.WardId, l.Address)).ToList(),
            p.BidPackages.Select(bp => new BidPackageDto(
                bp.Id, bp.Name, bp.ContractorSelectionPlanId,
                bp.IsDesignReview, bp.IsSupervision,
                bp.BidSelectionFormId, bp.BidSelectionMethodId,
                bp.ContractFormId, bp.BidSectorTypeId,
                bp.Duration, (int?)bp.DurationUnit,
                bp.EstimatedPrice, bp.WinningPrice, bp.WinningContractorId,
                bp.ResultDecisionNumber, bp.ResultDecisionDate, bp.ResultFileId, bp.Notes,
                bp.BidItems.Select(i => new BidItemDto(
                    i.Id, i.Name, i.Quantity, i.Unit, i.EstimatedPrice, i.Notes)).ToList(),
                bp.Contracts.Select(c => new ContractDto(
                    c.Id, c.ContractNumber, c.ContractDate, c.ContractorId,
                    c.ContractValue, c.ContractFormId, c.Notes, c.FileId)).ToList()
            )).ToList(),
            p.InspectionRecords.Select(i => new InspectionDto(
                i.Id, i.InspectionDate, i.InspectionAgency, i.Content,
                i.Conclusion, i.FileId)).ToList(),
            p.EvaluationRecords.Select(e => new EvaluationDto(
                e.Id, e.EvaluationDate, e.EvaluationTypeId, e.Content,
                e.Result, e.FileId)).ToList(),
            p.AuditRecords.Select(a => new AuditRecordDto(
                a.Id, a.AuditDate, a.AuditAgency, a.ConclusionTypeId,
                a.Content, a.FileId)).ToList(),
            p.ViolationRecords.Select(v => new ViolationDto(
                v.Id, v.ViolationDate, v.ViolationTypeId, v.Content,
                v.ViolationActionId, v.Penalty, v.Notes, v.FileId)).ToList(),
            p.Documents.Select(d => new ProjectDocumentDto(
                d.Id, d.DocumentTypeId, d.FileId, d.Title, d.UploadedAt, d.Notes)).ToList(),
            p.DesignEstimates.Select(de => new DesignEstimateDto(
                de.Id, de.ApprovalDecisionNumber, de.ApprovalDecisionDate,
                de.ApprovalAuthority, de.ApprovalSigner, de.ApprovalSummary, de.ApprovalFileId,
                de.EquipmentCost, de.ConstructionCost, de.LandCompensationCost,
                de.ManagementCost, de.ConsultancyCost, de.ContingencyCost,
                de.OtherCost, de.TotalEstimate, de.Notes,
                de.Items.Select(i => new DesignEstimateItemDto(
                    i.Id, i.ItemName, i.Scale, i.Cost, i.FileId)).ToList()
            )).ToList(),
            // PPP-specific child collections
            p.InvestmentDecisions.Select(d => new PppInvestmentDecisionDto(
                d.Id, (int)d.DecisionType, d.DecisionNumber, d.DecisionDate,
                d.DecisionAuthority, d.DecisionPerson,
                d.TotalInvestment, d.StateCapital, d.CentralBudget,
                d.LocalBudget, d.OtherStateBudget, d.EquityCapital, d.LoanCapital,
                d.EquityRatio, d.AdjustmentContentId, d.Notes, d.FileId)).ToList(),
            p.CapitalPlans.Select(c => new PppCapitalPlanDto(
                c.Id, c.DecisionType, c.DecisionNumber, c.DecisionDate,
                c.StateCapitalByDecision, c.FileId, c.Notes)).ToList(),
            p.ExecutionRecords.Select(e => new PppExecutionRecordDto(
                e.Id, e.ReportDate,
                e.ValueExecutedPeriod, e.ValueExecutedCumulative, e.CumulativeFromStart,
                e.SubProjectStateCapitalPeriod, e.SubProjectStateCapitalCumulative,
                e.BidPackageId, e.ContractId)).ToList(),
            p.DisbursementRecords.Select(d => new PppDisbursementRecordDto(
                d.Id, d.ReportDate,
                d.StateCapitalPeriod, d.StateCapitalCumulative,
                d.EquityCapitalPeriod, d.EquityCapitalCumulative,
                d.LoanCapitalPeriod, d.LoanCapitalCumulative)).ToList(),
            p.RevenueReports.Select(r => new RevenueReportDto(
                r.Id, r.ReportYear, r.ReportPeriod,
                r.RevenuePeriod, r.RevenueCumulative,
                r.RevenueIncreaseSharing, r.RevenueDecreaseSharing,
                r.Difficulties, r.Recommendations)).ToList(),
            // PPP 1-to-1 relations
            p.InvestorSelection is null ? null : new InvestorSelectionDto(
                p.InvestorSelection.ProjectId,
                p.InvestorSelection.SelectionMethod,
                p.InvestorSelection.SelectionDecisionNumber,
                p.InvestorSelection.SelectionDecisionDate,
                p.InvestorSelection.SelectionFileId,
                p.InvestorSelection.Investors
                    .OrderBy(i => i.SortOrder)
                    .Select(i => new InvestorSelectionInvestorDto(i.InvestorId, i.SortOrder))
                    .ToList()),
            p.ContractInfo is null ? null : new PppContractInfoDto(
                p.ContractInfo.ProjectId,
                p.ContractInfo.TotalInvestment, p.ContractInfo.StateCapital,
                p.ContractInfo.CentralBudget, p.ContractInfo.LocalBudget,
                p.ContractInfo.OtherStateBudget, p.ContractInfo.EquityCapital,
                p.ContractInfo.LoanCapital, p.ContractInfo.EquityRatio,
                p.ContractInfo.ImplementationProgress, p.ContractInfo.ContractDuration,
                p.ContractInfo.RevenueSharingMechanism, p.ContractInfo.ContractAuthority,
                p.ContractInfo.ContractNumber, p.ContractInfo.ContractDate,
                p.ContractInfo.ConstructionStartDate, p.ContractInfo.CompletionDate),
            p.RowVersion);
}
