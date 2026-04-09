namespace GSDT.InvestmentProjects.Application.DTOs;

/// <summary>List item DTO for ODA project grid/search results.</summary>
public sealed record OdaProjectListItemDto(
    Guid Id,
    string ProjectCode,
    string ProjectName,
    string ShortName,
    string? OdaProjectTypeName,
    DateTimeOffset CreatedAt,
    string? StatusName);

/// <summary>Full detail DTO for ODA project view — includes all child collections.</summary>
public sealed record OdaProjectDetailDto(
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
    // ODA-specific
    string ShortName,
    string? ProjectCodeQhns,
    Guid OdaProjectTypeId,
    Guid DonorId,
    string? CoDonorName,
    decimal OdaGrantCapital,
    decimal OdaLoanCapital,
    decimal CounterpartCentralBudget,
    decimal CounterpartLocalBudget,
    decimal CounterpartOtherCapital,
    decimal TotalInvestment,
    decimal GrantMechanismPercent,
    decimal RelendingMechanismPercent,
    Guid StatusId,
    bool ProcurementConditionBound,
    string? ProcurementConditionSummary,
    int? StartYear,
    int? EndYear,
    // Shared child collections
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
    IReadOnlyList<ProjectDocumentDto> Documents,
    // ODA-only child collections
    IReadOnlyList<LoanAgreementDto> LoanAgreements,
    IReadOnlyList<ServiceBankDto> ServiceBanks,
    ProcurementConditionDto? ProcurementCondition);
