namespace GSDT.InvestmentProjects.Application.Commands.CreateFdiProject;

/// <summary>Creates a new FDI (foreign direct investment) project aggregate. Returns the new project Id.</summary>
public sealed record CreateFdiProjectCommand(
    string ProjectCode,
    string ProjectName,
    Guid ManagingAuthorityId,
    Guid IndustrySectorId,
    Guid ProjectOwnerId,
    Guid ProjectGroupId,
    SubProjectType SubProjectType,
    Guid StatusId,
    // FDI-specific (same as NĐT/DNNN)
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
