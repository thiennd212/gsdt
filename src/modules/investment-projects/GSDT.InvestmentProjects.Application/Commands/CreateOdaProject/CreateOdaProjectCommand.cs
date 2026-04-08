namespace GSDT.InvestmentProjects.Application.Commands.CreateOdaProject;

/// <summary>Creates a new ODA-funded investment project. Returns the new project Id.</summary>
public sealed record CreateOdaProjectCommand(
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
    // Policy decision (optional at creation)
    string? PolicyDecisionNumber,
    DateTime? PolicyDecisionDate,
    string? PolicyDecisionAuthority,
    string? PolicyDecisionPerson,
    Guid? PolicyDecisionFileId
) : ICommand<Guid>;
