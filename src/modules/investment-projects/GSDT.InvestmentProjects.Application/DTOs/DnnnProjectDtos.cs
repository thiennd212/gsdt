namespace GSDT.InvestmentProjects.Application.DTOs;

/// <summary>Compact list item DTO for DNNN project grid/search results.</summary>
public sealed record DnnnProjectListItemDto(
    Guid Id,
    string ProjectCode,
    string ProjectName,
    string? InvestorName,
    string? CompetentAuthorityName,
    decimal PrelimTotalInvestment,
    string? StatusName,
    DateTime CreatedAt);

/// <summary>Full detail DTO for DNNN project view — includes all child collections.</summary>
public sealed record DnnnProjectDetailDto(
    Guid Id,
    string ProjectCode,
    string ProjectName,
    // DNNN identity
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
    // Capital estimates — DNNN structure
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
    IReadOnlyList<DesignEstimateDto> DesignEstimates,
    IReadOnlyList<RegistrationCertificateDto> RegistrationCertificates,
    // DNNN-specific child collections
    IReadOnlyList<DnnnInvestmentDecisionDto> InvestmentDecisions,
    // Shared 1-to-1
    InvestorSelectionDto? InvestorSelection,
    byte[] RowVersion);

// ── DNNN-specific sub-entity DTOs ────────────────────────────────────────────

/// <summary>Investment decision record for a DNNN project (CSH/ODA/TCTD capital).</summary>
public sealed record DnnnInvestmentDecisionDto(
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

/// <summary>Registration certificate (GCNĐKĐT) — shared by DNNN/NĐT/FDI.</summary>
public sealed record RegistrationCertificateDto(
    Guid Id,
    string CertificateNumber,
    DateTime IssuedDate,
    Guid? FileId,
    decimal InvestmentCapital,
    decimal EquityCapital,
    decimal? EquityRatio,
    string? Notes);
