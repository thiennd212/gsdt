namespace GSDT.InvestmentProjects.Application.DTOs;

/// <summary>List item DTO for domestic project grid/search results.</summary>
public sealed record DomesticProjectListItemDto(
    Guid Id,
    string ProjectCode,
    string ProjectName,
    string? LatestDecisionNumber,
    DateTime? LatestDecisionDate,
    string? ProjectOwnerName,
    string? StatusName);

/// <summary>Full detail DTO for domestic project view — includes all child collections.</summary>
public sealed record DomesticProjectDetailDto(
    Guid Id,
    string ProjectCode,
    string ProjectName,
    // Base identifiers
    Guid ManagingAuthorityId,
    Guid IndustrySectorId,
    Guid ProjectOwnerId,
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
    // Domestic-specific
    int SubProjectType,
    string? TreasuryCode,
    Guid ProjectGroupId,
    decimal PrelimCentralBudget,
    decimal PrelimLocalBudget,
    decimal PrelimOtherPublicCapital,
    decimal PrelimPublicInvestment,
    decimal PrelimOtherCapital,
    decimal PrelimTotalInvestment,
    Guid StatusId,
    Guid? NationalTargetProgramId,
    string? StopContent,
    string? StopDecisionNumber,
    DateTime? StopDecisionDate,
    Guid? StopFileId,
    // Child collections
    IReadOnlyList<ProjectLocationDto> Locations,
    IReadOnlyList<DecisionDto> Decisions,
    IReadOnlyList<CapitalPlanDto> CapitalPlans,
    IReadOnlyList<BidPackageDto> BidPackages,
    IReadOnlyList<ExecutionDto> Executions,
    IReadOnlyList<DisbursementDto> Disbursements,
    IReadOnlyList<InspectionDto> Inspections,
    IReadOnlyList<EvaluationDto> Evaluations,
    IReadOnlyList<AuditRecordDto> Audits,
    IReadOnlyList<ViolationDto> Violations,
    OperationInfoDto? Operation,
    IReadOnlyList<ProjectDocumentDto> Documents);
