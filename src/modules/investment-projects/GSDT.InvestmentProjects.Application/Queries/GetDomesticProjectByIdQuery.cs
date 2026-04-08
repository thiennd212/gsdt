namespace GSDT.InvestmentProjects.Application.Queries;

/// <summary>
/// Returns full detail of a single domestic project with all child collections.
/// Delegates eager-loading to IInvestmentProjectRepository to avoid Application → Infrastructure dependency.
/// </summary>
public sealed record GetDomesticProjectByIdQuery(Guid Id)
    : IRequest<Result<DomesticProjectDetailDto>>;

public sealed class GetDomesticProjectByIdQueryHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext)
    : IRequestHandler<GetDomesticProjectByIdQuery, Result<DomesticProjectDetailDto>>
{
    public async Task<Result<DomesticProjectDetailDto>> Handle(
        GetDomesticProjectByIdQuery request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail<DomesticProjectDetailDto>("GOV_INV_401: Tenant context is required.");

        var project = await repository.GetDomesticByIdWithDetailsAsync(
            request.Id, tenantId, cancellationToken);

        if (project is null)
            return Result.Fail<DomesticProjectDetailDto>(
                $"GOV_INV_404: Du an khong ton tai (Id={request.Id}).");

        return Result.Ok(MapToDetailDto(project));
    }

    private static DomesticProjectDetailDto MapToDetailDto(DomesticProject p) =>
        new(
            p.Id, p.ProjectCode, p.ProjectName,
            p.ManagingAuthorityId, p.IndustrySectorId, p.ProjectOwnerId,
            p.ProjectManagementUnitId, p.PmuDirectorName, p.PmuPhone, p.PmuEmail,
            p.ImplementationPeriod,
            p.PolicyDecisionNumber, p.PolicyDecisionDate, p.PolicyDecisionAuthority,
            p.PolicyDecisionPerson, p.PolicyDecisionFileId,
            (int)p.SubProjectType, p.TreasuryCode, p.ProjectGroupId,
            p.PrelimCentralBudget, p.PrelimLocalBudget, p.PrelimOtherPublicCapital,
            p.PrelimPublicInvestment, p.PrelimOtherCapital, p.PrelimTotalInvestment,
            p.StatusId, p.NationalTargetProgramId,
            p.StopContent, p.StopDecisionNumber, p.StopDecisionDate, p.StopFileId,
            p.Locations.Select(l => new ProjectLocationDto(
                l.Id, l.ProvinceId, l.DistrictId, l.WardId, l.Address)).ToList(),
            p.InvestmentDecisions.Select(d => new DecisionDto(
                d.Id, (int)d.DecisionType, d.DecisionNumber, d.DecisionDate,
                d.DecisionAuthority, d.TotalInvestment, d.CentralBudget,
                d.LocalBudget, d.OtherPublicCapital, d.OtherCapital,
                d.AdjustmentContentId, d.Notes, d.FileId)).ToList(),
            p.CapitalPlans.Select(c => new CapitalPlanDto(
                c.Id, (int)c.DecisionType, c.AllocationRound, c.DecisionNumber,
                c.DecisionDate, c.TotalAmount, c.CentralBudget, c.LocalBudget,
                c.Notes, c.FileId)).ToList(),
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
            p.ExecutionRecords.Select(e => new ExecutionDto(
                e.Id, e.ReportDate, e.BidPackageId, e.ContractId,
                (int)e.ProgressStatus, e.PhysicalProgressPercent, e.Notes)).ToList(),
            p.DisbursementRecords.Select(d => new DisbursementDto(
                d.Id, d.ReportDate, d.BidPackageId, d.ContractId,
                d.PublicCapitalMonthly, d.PublicCapitalPreviousMonth,
                d.PublicCapitalYtd, d.OtherCapital)).ToList(),
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
            p.OperationInfo is null ? null : new OperationInfoDto(
                p.OperationInfo.OperationDate, p.OperationInfo.OperatingAgency,
                p.OperationInfo.RevenueLastYear, p.OperationInfo.ExpenseLastYear,
                p.OperationInfo.Notes),
            p.Documents.Select(d => new ProjectDocumentDto(
                d.Id, d.DocumentTypeId, d.FileId, d.Title, d.UploadedAt, d.Notes)).ToList());
}
