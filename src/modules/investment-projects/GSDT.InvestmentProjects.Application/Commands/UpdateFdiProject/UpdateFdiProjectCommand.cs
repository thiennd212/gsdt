namespace GSDT.InvestmentProjects.Application.Commands.UpdateFdiProject;

/// <summary>Updates an existing FDI project. RowVersion enforces optimistic concurrency.</summary>
public sealed record UpdateFdiProjectCommand(
    Guid Id,
    byte[] RowVersion,
    string ProjectCode,
    string ProjectName,
    Guid ManagingAuthorityId,
    Guid IndustrySectorId,
    Guid ProjectOwnerId,
    Guid ProjectGroupId,
    SubProjectType SubProjectType,
    Guid StatusId,
    // FDI-specific
    Guid? CompetentAuthorityId,
    string? InvestorName,
    decimal? StateOwnershipRatio,
    string? Objective,
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
