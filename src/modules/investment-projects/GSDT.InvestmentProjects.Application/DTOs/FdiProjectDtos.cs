namespace GSDT.InvestmentProjects.Application.DTOs;

/// <summary>Compact list item DTO for FDI project grid/search results.</summary>
public sealed record FdiProjectListItemDto(
    Guid Id,
    string ProjectCode,
    string ProjectName,
    string? InvestorName,
    string? CompetentAuthorityName,
    decimal PrelimTotalInvestment,
    string? StatusName,
    DateTimeOffset CreatedAt);

/// <summary>Full detail DTO for FDI project view — NO DesignEstimates, NO InvestorSelection.</summary>
public sealed record FdiProjectDetailDto(
    Guid Id,
    string ProjectCode,
    string ProjectName,
    int SubProjectType,
    Guid ProjectGroupId,
    Guid StatusId,
    Guid ManagingAuthorityId,
    Guid IndustrySectorId,
    Guid ProjectOwnerId,
    Guid? CompetentAuthorityId,
    string? InvestorName,
    decimal? StateOwnershipRatio,
    string? Objective,
    // PMU
    Guid? ProjectManagementUnitId,
    string? PmuDirectorName,
    string? PmuPhone,
    string? PmuEmail,
    string? ImplementationPeriod,
    // Policy decision
    string? PolicyDecisionNumber,
    DateTime? PolicyDecisionDate,
    string? PolicyDecisionAuthority,
    string? PolicyDecisionPerson,
    Guid? PolicyDecisionFileId,
    // Capital estimates
    decimal PrelimTotalInvestment,
    decimal PrelimEquityCapital,
    decimal PrelimOdaLoanCapital,
    decimal PrelimCreditLoanCapital,
    decimal? AreaHectares,
    string? Capacity,
    string? MainItems,
    string? ImplementationTimeline,
    string? ProgressDescription,
    // Suspension
    string? StopContent,
    string? StopDecisionNumber,
    DateTime? StopDecisionDate,
    Guid? StopFileId,
    // Shared child collections
    IReadOnlyList<ProjectLocationDto> Locations,
    IReadOnlyList<BidPackageDto> BidPackages,
    IReadOnlyList<InspectionDto> Inspections,
    IReadOnlyList<EvaluationDto> Evaluations,
    IReadOnlyList<AuditRecordDto> Audits,
    IReadOnlyList<ViolationDto> Violations,
    IReadOnlyList<ProjectDocumentDto> Documents,
    IReadOnlyList<RegistrationCertificateDto> RegistrationCertificates,
    // FDI-specific child collections
    IReadOnlyList<FdiInvestmentDecisionDto> InvestmentDecisions,
    byte[] RowVersion);

/// <summary>Investment decision record for an FDI project (CSH/ODA/TCTD capital).</summary>
public sealed record FdiInvestmentDecisionDto(
    Guid Id,
    int DecisionType,
    string DecisionNumber,
    DateTime DecisionDate,
    string DecisionAuthority,
    string? DecisionPerson,
    decimal TotalInvestment,
    decimal EquityCapital,
    decimal OdaLoanCapital,
    decimal CreditLoanCapital,
    decimal? EquityRatio,
    Guid? AdjustmentContentId,
    string? Notes,
    Guid? FileId);
