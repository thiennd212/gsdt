namespace GSDT.InvestmentProjects.Application.Queries;

/// <summary>
/// Returns full detail of a single ODA project with all child collections.
/// Delegates eager-loading to IInvestmentProjectRepository to avoid Application → Infrastructure dependency.
/// ODA disbursement fields map MonthlyTotal → PublicCapitalMonthly, YtdTotal → PublicCapitalYtd
/// in the shared DisbursementDto (full ODA breakdown available via dedicated ODA disbursement DTO in future).
/// </summary>
public sealed record GetOdaProjectByIdQuery(Guid Id)
    : IRequest<Result<OdaProjectDetailDto>>;

public sealed class GetOdaProjectByIdQueryHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext)
    : IRequestHandler<GetOdaProjectByIdQuery, Result<OdaProjectDetailDto>>
{
    public async Task<Result<OdaProjectDetailDto>> Handle(
        GetOdaProjectByIdQuery request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail<OdaProjectDetailDto>("GOV_INV_401: Tenant context is required.");

        var project = await repository.GetOdaByIdWithDetailsAsync(
            request.Id, tenantId, cancellationToken);

        if (project is null)
            return Result.Fail<OdaProjectDetailDto>(
                $"GOV_INV_404: Du an ODA khong ton tai (Id={request.Id}).");

        return Result.Ok(MapToDetailDto(project));
    }

    private static OdaProjectDetailDto MapToDetailDto(OdaProject p) =>
        new(
            p.Id, p.ProjectCode, p.ProjectName,
            p.ManagingAuthorityId, p.IndustrySectorId, p.ProjectOwnerId,
            p.ProjectManagementUnitId, p.PmuDirectorName, p.PmuPhone, p.PmuEmail,
            p.ImplementationPeriod,
            p.PolicyDecisionNumber, p.PolicyDecisionDate, p.PolicyDecisionAuthority,
            p.PolicyDecisionPerson, p.PolicyDecisionFileId,
            p.ShortName, p.ProjectCodeQhns,
            p.OdaProjectTypeId, p.DonorId, p.CoDonorName,
            p.OdaGrantCapital, p.OdaLoanCapital,
            p.CounterpartCentralBudget, p.CounterpartLocalBudget, p.CounterpartOtherCapital,
            p.TotalInvestment, p.GrantMechanismPercent, p.RelendingMechanismPercent,
            p.StatusId, p.ProcurementConditionBound, p.ProcurementConditionSummary,
            p.StartYear, p.EndYear,
            // Shared child collections
            p.Locations.Select(l => new ProjectLocationDto(
                l.Id, l.ProvinceId, l.DistrictId, l.WardId, l.Address)).ToList(),
            // ODA decisions: map ODA capital fields to shared DecisionDto
            // CentralBudget → CounterpartCentralBudget, LocalBudget → CounterpartLocalBudget
            // OtherPublicCapital → CounterpartOtherCapital, OtherCapital = 0 (not applicable)
            p.InvestmentDecisions.Select(d => new DecisionDto(
                d.Id, (int)d.DecisionType, d.DecisionNumber, d.DecisionDate,
                d.DecisionAuthority, d.TotalInvestment,
                d.CounterpartCentralBudget, d.CounterpartLocalBudget,
                d.CounterpartOtherCapital, 0m,
                d.AdjustmentContentId, d.Notes, d.FileId)).ToList(),
            // ODA capital plans: TotalAmount + counterpart central/local mapped to shared CapitalPlanDto
            p.CapitalPlans.Select(c => new CapitalPlanDto(
                c.Id, (int)c.DecisionType, c.AllocationRound, c.DecisionNumber,
                c.DecisionDate, c.TotalAmount,
                c.CounterpartCentral, c.CounterpartLocal,
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
            // ODA disbursement: MonthlyTotal → PublicCapitalMonthly, YtdTotal → PublicCapitalYtd
            p.DisbursementRecords.Select(d => new DisbursementDto(
                d.Id, d.ReportDate, d.BidPackageId, d.ContractId,
                d.MonthlyTotal, null, d.YtdTotal, null)).ToList(),
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
                d.Id, d.DocumentTypeId, d.FileId, d.Title, d.UploadedAt, d.Notes)).ToList(),
            // ODA-only child collections
            p.LoanAgreements.Select(la => new LoanAgreementDto(
                la.Id, la.AgreementNumber, la.AgreementDate, la.LenderName,
                la.Amount, la.Currency, la.InterestRate,
                la.GracePeriod, la.RepaymentPeriod, la.Notes, la.FileId)).ToList(),
            p.ServiceBanks.Select(sb => new ServiceBankDto(
                sb.Id, sb.BankId, sb.Role, sb.Notes)).ToList(),
            p.ProcurementCondition is null ? null : new ProcurementConditionDto(
                p.ProcurementCondition.IsBound, p.ProcurementCondition.Summary,
                p.ProcurementCondition.DonorApprovalRequired,
                p.ProcurementCondition.SpecialConditions));
}
