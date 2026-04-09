namespace GSDT.InvestmentProjects.Application.Queries;

/// <summary>Returns full detail of a single FDI project with all child collections.</summary>
public sealed record GetFdiProjectByIdQuery(Guid Id)
    : IRequest<Result<FdiProjectDetailDto>>;

public sealed class GetFdiProjectByIdQueryHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext)
    : IRequestHandler<GetFdiProjectByIdQuery, Result<FdiProjectDetailDto>>
{
    public async Task<Result<FdiProjectDetailDto>> Handle(
        GetFdiProjectByIdQuery request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail<FdiProjectDetailDto>("GOV_INV_401: Tenant context is required.");

        var project = await repository.GetFdiByIdWithDetailsAsync(
            request.Id, tenantId, cancellationToken);

        if (project is null)
            return Result.Fail<FdiProjectDetailDto>(
                $"GOV_INV_404: Du an FDI khong ton tai (Id={request.Id}).");

        return Result.Ok(MapToDetailDto(project));
    }

    private static FdiProjectDetailDto MapToDetailDto(FdiProject p) =>
        new(
            p.Id, p.ProjectCode, p.ProjectName,
            (int)p.SubProjectType,
            p.ProjectGroupId, p.StatusId,
            p.ManagingAuthorityId, p.IndustrySectorId, p.ProjectOwnerId,
            p.CompetentAuthorityId, p.InvestorName, p.StateOwnershipRatio, p.Objective,
            p.ProjectManagementUnitId, p.PmuDirectorName, p.PmuPhone, p.PmuEmail,
            p.ImplementationPeriod,
            p.PolicyDecisionNumber, p.PolicyDecisionDate,
            p.PolicyDecisionAuthority, p.PolicyDecisionPerson, p.PolicyDecisionFileId,
            p.PrelimTotalInvestment, p.PrelimEquityCapital,
            p.PrelimOdaLoanCapital, p.PrelimCreditLoanCapital,
            p.AreaHectares, p.Capacity, p.MainItems,
            p.ImplementationTimeline, p.ProgressDescription,
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
            p.RegistrationCertificates.Select(c => new RegistrationCertificateDto(
                c.Id, c.CertificateNumber, c.IssuedDate, c.FileId,
                c.InvestmentCapital, c.EquityCapital, c.EquityRatio, c.Notes)).ToList(),
            // FDI-specific — NO DesignEstimates, NO InvestorSelection
            p.InvestmentDecisions.Select(d => new FdiInvestmentDecisionDto(
                d.Id, (int)d.DecisionType, d.DecisionNumber, d.DecisionDate,
                d.DecisionAuthority, d.DecisionPerson,
                d.TotalInvestment, d.EquityCapital,
                d.OdaLoanCapital, d.CreditLoanCapital,
                d.EquityRatio, d.AdjustmentContentId, d.Notes, d.FileId)).ToList(),
            p.RowVersion);
}
