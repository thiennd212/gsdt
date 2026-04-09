namespace GSDT.InvestmentProjects.Application.DTOs;

/// <summary>Compact list item DTO for PPP project grid/search results.</summary>
public sealed record PppProjectListItemDto(
    Guid Id,
    string ProjectCode,
    string ProjectName,
    int ContractType,
    string? CompetentAuthorityName,
    string? PreparationUnit,
    decimal PrelimTotalInvestment,
    string? StatusName,
    DateTimeOffset CreatedAt);

/// <summary>Full detail DTO for PPP project view — includes all child collections.</summary>
public sealed record PppProjectDetailDto(
    Guid Id,
    string ProjectCode,
    string ProjectName,
    // PPP identity
    int ContractType,
    int SubProjectType,
    Guid ProjectGroupId,
    Guid StatusId,
    Guid ManagingAuthorityId,
    Guid IndustrySectorId,
    Guid ProjectOwnerId,
    Guid? CompetentAuthorityId,
    string? PreparationUnit,
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
    decimal PrelimStateCapital,
    decimal PrelimEquityCapital,
    decimal PrelimLoanCapital,
    decimal? AreaHectares,
    string? Capacity,
    string? MainItems,
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
    // PPP-specific child collections
    IReadOnlyList<PppInvestmentDecisionDto> InvestmentDecisions,
    IReadOnlyList<PppCapitalPlanDto> CapitalPlans,
    IReadOnlyList<PppExecutionRecordDto> ExecutionRecords,
    IReadOnlyList<PppDisbursementRecordDto> DisbursementRecords,
    IReadOnlyList<RevenueReportDto> RevenueReports,
    // PPP 1-to-1 relations
    InvestorSelectionDto? InvestorSelection,
    PppContractInfoDto? ContractInfo,
    byte[] RowVersion);

// ── PPP-specific sub-entity DTOs ──────────────────────────────────────────────

/// <summary>Investment decision record for a PPP project.</summary>
public sealed record PppInvestmentDecisionDto(
    Guid Id,
    int DecisionType,
    string DecisionNumber,
    DateTime DecisionDate,
    string DecisionAuthority,
    string? DecisionPerson,
    decimal TotalInvestment,
    decimal StateCapital,
    decimal CentralBudget,
    decimal LocalBudget,
    decimal OtherStateBudget,
    decimal EquityCapital,
    decimal LoanCapital,
    decimal? EquityRatio,
    Guid? AdjustmentContentId,
    string? Notes,
    Guid? FileId);

/// <summary>Capital plan decision for a PPP project.</summary>
public sealed record PppCapitalPlanDto(
    Guid Id,
    string DecisionType,
    string DecisionNumber,
    DateTime DecisionDate,
    decimal StateCapitalByDecision,
    Guid? FileId,
    string? Notes);

/// <summary>Execution progress snapshot for a PPP project.</summary>
public sealed record PppExecutionRecordDto(
    Guid Id,
    DateTime ReportDate,
    decimal ValueExecutedPeriod,
    decimal ValueExecutedCumulative,
    decimal CumulativeFromStart,
    decimal? SubProjectStateCapitalPeriod,
    decimal? SubProjectStateCapitalCumulative,
    Guid? BidPackageId,
    Guid? ContractId);

/// <summary>Disbursement snapshot for a PPP project.</summary>
public sealed record PppDisbursementRecordDto(
    Guid Id,
    DateTime ReportDate,
    decimal StateCapitalPeriod,
    decimal StateCapitalCumulative,
    decimal EquityCapitalPeriod,
    decimal EquityCapitalCumulative,
    decimal LoanCapitalPeriod,
    decimal LoanCapitalCumulative);

/// <summary>Revenue sharing report for a PPP project.</summary>
public sealed record RevenueReportDto(
    Guid Id,
    int ReportYear,
    string ReportPeriod,
    decimal RevenuePeriod,
    decimal RevenueCumulative,
    decimal? RevenueIncreaseSharing,
    decimal? RevenueDecreaseSharing,
    string? Difficulties,
    string? Recommendations);

/// <summary>Investor selection record with linked investor list.</summary>
public sealed record InvestorSelectionDto(
    Guid ProjectId,
    string? SelectionMethod,
    string? SelectionDecisionNumber,
    DateTime? SelectionDecisionDate,
    Guid? SelectionFileId,
    IReadOnlyList<InvestorSelectionInvestorDto> Investors);

/// <summary>Junction record linking an investor to a selection.</summary>
public sealed record InvestorSelectionInvestorDto(
    Guid InvestorId,
    int SortOrder);

/// <summary>Finalized PPP contract information.</summary>
public sealed record PppContractInfoDto(
    Guid ProjectId,
    decimal TotalInvestment,
    decimal StateCapital,
    decimal CentralBudget,
    decimal LocalBudget,
    decimal OtherStateBudget,
    decimal EquityCapital,
    decimal LoanCapital,
    decimal? EquityRatio,
    string? ImplementationProgress,
    string? ContractDuration,
    string? RevenueSharingMechanism,
    string? ContractAuthority,
    string? ContractNumber,
    DateTime? ContractDate,
    DateTime? ConstructionStartDate,
    DateTime? CompletionDate);

/// <summary>Design estimate with line items — used by PPP and DNNN projects.</summary>
public sealed record DesignEstimateDto(
    Guid Id,
    string? ApprovalDecisionNumber,
    DateTime? ApprovalDecisionDate,
    string? ApprovalAuthority,
    string? ApprovalSigner,
    string? ApprovalSummary,
    Guid? ApprovalFileId,
    decimal EquipmentCost,
    decimal ConstructionCost,
    decimal LandCompensationCost,
    decimal ManagementCost,
    decimal ConsultancyCost,
    decimal ContingencyCost,
    decimal OtherCost,
    decimal TotalEstimate,
    string? Notes,
    IReadOnlyList<DesignEstimateItemDto> Items);

/// <summary>Line item within a design estimate.</summary>
public sealed record DesignEstimateItemDto(
    Guid Id,
    string ItemName,
    string? Scale,
    decimal Cost,
    Guid? FileId);
