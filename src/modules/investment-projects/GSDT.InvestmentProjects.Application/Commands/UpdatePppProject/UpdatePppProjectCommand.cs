namespace GSDT.InvestmentProjects.Application.Commands.UpdatePppProject;

/// <summary>Updates an existing PPP project. RowVersion enforces optimistic concurrency.</summary>
public sealed record UpdatePppProjectCommand(
    Guid Id,
    byte[] RowVersion,
    string ProjectCode,
    string ProjectName,
    Guid ManagingAuthorityId,
    Guid IndustrySectorId,
    Guid ProjectOwnerId,
    Guid ProjectGroupId,
    PppContractType ContractType,
    SubProjectType SubProjectType,
    Guid StatusId,
    // Optional fields
    Guid? CompetentAuthorityId,
    string? PreparationUnit,
    string? Objective,
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
    Guid? PolicyDecisionFileId
) : ICommand;
