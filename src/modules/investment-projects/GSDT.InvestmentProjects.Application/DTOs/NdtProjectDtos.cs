namespace GSDT.InvestmentProjects.Application.DTOs;

/// <summary>Compact list item DTO for NĐT project grid/search results.</summary>
public sealed record NdtProjectListItemDto(
    Guid Id,
    string ProjectCode,
    string ProjectName,
    string? InvestorName,
    string? CompetentAuthorityName,
    decimal PrelimTotalInvestment,
    string? StatusName,
    DateTimeOffset CreatedAt);

/// <summary>Full detail DTO for NĐT project view — NO DesignEstimates, NO InvestorSelection.</summary>
public sealed record NdtProjectDetailDto(
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
    // NĐT-specific child collections
    IReadOnlyList<NdtInvestmentDecisionDto> InvestmentDecisions,
    byte[] RowVersion);

/// <summary>Investment decision record for an NĐT project (CSH/ODA/TCTD capital).</summary>
public sealed record NdtInvestmentDecisionDto(
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
