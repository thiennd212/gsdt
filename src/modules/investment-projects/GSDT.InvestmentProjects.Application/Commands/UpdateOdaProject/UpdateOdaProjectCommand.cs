namespace GSDT.InvestmentProjects.Application.Commands.UpdateOdaProject;

/// <summary>Updates an existing ODA project. RowVersion enforces optimistic concurrency.</summary>
public sealed record UpdateOdaProjectCommand(
    Guid Id,
    byte[] RowVersion,
    string ProjectCode,
    string ProjectName,
    string ShortName,
    Guid ManagingAuthorityId,
    Guid IndustrySectorId,
    Guid ProjectOwnerId,
    Guid OdaProjectTypeId,
    Guid DonorId,
    // PMU (optional)
    Guid? ProjectManagementUnitId,
    string? PmuDirectorName,
    string? PmuPhone,
    string? PmuEmail,
    string? ImplementationPeriod,
    // ODA capital breakdown
    decimal OdaGrantCapital,
    decimal OdaLoanCapital,
    decimal CounterpartCentralBudget,
    decimal CounterpartLocalBudget,
    decimal CounterpartOtherCapital,
    // Mechanism percentages
    decimal GrantMechanismPercent,
    decimal RelendingMechanismPercent,
    // Status & classification
    Guid StatusId,
    bool ProcurementConditionBound,
    string? ProcurementConditionSummary,
    int? StartYear,
    int? EndYear,
    // Optional fields (v1.1)
    string? ProjectCodeQhns,
    string? CoDonorName,
    // Policy decision
    string? PolicyDecisionNumber,
    DateTime? PolicyDecisionDate,
    string? PolicyDecisionAuthority,
    string? PolicyDecisionPerson,
    Guid? PolicyDecisionFileId
) : ICommand;
