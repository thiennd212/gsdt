namespace GSDT.InvestmentProjects.Application.Commands.CreatePppProject;

/// <summary>Creates a new PPP project aggregate. Returns the new project Id.</summary>
public sealed record CreatePppProjectCommand(
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
) : ICommand<Guid>;
