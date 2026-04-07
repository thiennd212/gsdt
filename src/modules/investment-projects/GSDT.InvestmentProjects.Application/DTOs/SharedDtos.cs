namespace GSDT.InvestmentProjects.Application.DTOs;

/// <summary>Geographic location entry for a project.</summary>
public sealed record ProjectLocationDto(
    Guid Id,
    Guid ProvinceId,
    Guid? DistrictId,
    Guid? WardId,
    string? Address);

/// <summary>Investment decision (initial or adjustment) for a project.</summary>
public sealed record DecisionDto(
    Guid Id,
    int DecisionType,
    string DecisionNumber,
    DateTime DecisionDate,
    string DecisionAuthority,
    decimal TotalInvestment,
    decimal CentralBudget,
    decimal LocalBudget,
    decimal OtherPublicCapital,
    decimal OtherCapital,
    Guid? AdjustmentContentId,
    string? Notes,
    Guid? FileId);

/// <summary>Capital allocation plan decision.</summary>
public sealed record CapitalPlanDto(
    Guid Id,
    int DecisionType,
    int AllocationRound,
    string DecisionNumber,
    DateTime DecisionDate,
    decimal TotalAmount,
    decimal CentralBudget,
    decimal LocalBudget,
    string? Notes,
    Guid? FileId);

/// <summary>Bid package with nested items and contracts.</summary>
public sealed record BidPackageDto(
    Guid Id,
    string Name,
    Guid? ContractorSelectionPlanId,
    bool IsDesignReview,
    bool IsSupervision,
    Guid BidSelectionFormId,
    Guid BidSelectionMethodId,
    Guid ContractFormId,
    Guid BidSectorTypeId,
    int? Duration,
    int? DurationUnit,
    decimal? EstimatedPrice,
    decimal? WinningPrice,
    Guid? WinningContractorId,
    string? ResultDecisionNumber,
    DateTime? ResultDecisionDate,
    Guid? ResultFileId,
    string? Notes,
    IReadOnlyList<BidItemDto> Items,
    IReadOnlyList<ContractDto> Contracts);

/// <summary>Line item within a bid package.</summary>
public sealed record BidItemDto(
    Guid Id,
    string Name,
    decimal? Quantity,
    string? Unit,
    decimal? EstimatedPrice,
    string? Notes);

/// <summary>Contract awarded under a bid package.</summary>
public sealed record ContractDto(
    Guid Id,
    string ContractNumber,
    DateTime ContractDate,
    Guid ContractorId,
    decimal ContractValue,
    Guid ContractFormId,
    string? Notes,
    Guid? FileId);

/// <summary>Execution progress snapshot.</summary>
public sealed record ExecutionDto(
    Guid Id,
    DateTime ReportDate,
    Guid? BidPackageId,
    Guid? ContractId,
    int ProgressStatus,
    decimal? PhysicalProgressPercent,
    string? Notes);

/// <summary>Disbursement snapshot.</summary>
public sealed record DisbursementDto(
    Guid Id,
    DateTime ReportDate,
    Guid? BidPackageId,
    Guid? ContractId,
    decimal PublicCapitalMonthly,
    decimal? PublicCapitalPreviousMonth,
    decimal PublicCapitalYtd,
    decimal? OtherCapital);

/// <summary>Inspection visit record.</summary>
public sealed record InspectionDto(
    Guid Id,
    DateTime InspectionDate,
    string InspectionAgency,
    string Content,
    string? Conclusion,
    Guid? FileId);

/// <summary>Mid-term or final evaluation record.</summary>
public sealed record EvaluationDto(
    Guid Id,
    DateTime EvaluationDate,
    Guid EvaluationTypeId,
    string Content,
    string? Result,
    Guid? FileId);

/// <summary>State audit record.</summary>
public sealed record AuditRecordDto(
    Guid Id,
    DateTime AuditDate,
    string AuditAgency,
    Guid ConclusionTypeId,
    string Content,
    Guid? FileId);

/// <summary>Regulatory violation record.</summary>
public sealed record ViolationDto(
    Guid Id,
    DateTime ViolationDate,
    Guid ViolationTypeId,
    string Content,
    Guid ViolationActionId,
    decimal? Penalty,
    string? Notes,
    Guid? FileId);

/// <summary>Post-completion operational information.</summary>
public sealed record OperationInfoDto(
    DateTime? OperationDate,
    string? OperatingAgency,
    decimal? RevenueLastYear,
    decimal? ExpenseLastYear,
    string? Notes);

/// <summary>Document attachment for a project.</summary>
public sealed record ProjectDocumentDto(
    Guid Id,
    Guid DocumentTypeId,
    Guid FileId,
    string Title,
    DateTime UploadedAt,
    string? Notes);

/// <summary>Loan agreement for an ODA project.</summary>
public sealed record LoanAgreementDto(
    Guid Id,
    string AgreementNumber,
    DateTime AgreementDate,
    string LenderName,
    decimal Amount,
    string Currency,
    decimal? InterestRate,
    int? GracePeriod,
    int? RepaymentPeriod,
    string? Notes,
    Guid? FileId);

/// <summary>Service bank for an ODA project.</summary>
public sealed record ServiceBankDto(
    Guid Id,
    Guid BankId,
    string Role,
    string? Notes);

/// <summary>Donor-imposed procurement conditions for an ODA project.</summary>
public sealed record ProcurementConditionDto(
    bool IsBound,
    string? Summary,
    bool DonorApprovalRequired,
    string? SpecialConditions);
