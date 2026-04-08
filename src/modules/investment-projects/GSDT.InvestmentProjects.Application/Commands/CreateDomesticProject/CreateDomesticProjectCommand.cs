namespace GSDT.InvestmentProjects.Application.Commands.CreateDomesticProject;

/// <summary>Creates a new domestic public-investment project. Returns the new project Id.</summary>
public sealed record CreateDomesticProjectCommand(
    string ProjectCode,
    string ProjectName,
    Guid ManagingAuthorityId,
    Guid IndustrySectorId,
    Guid ProjectOwnerId,
    Guid ProjectGroupId,
    SubProjectType SubProjectType,
    // PMU (optional)
    Guid? ProjectManagementUnitId,
    string? PmuDirectorName,
    string? PmuPhone,
    string? PmuEmail,
    string? ImplementationPeriod,
    // Preliminary capital estimates
    decimal PrelimCentralBudget,
    decimal PrelimLocalBudget,
    decimal PrelimOtherPublicCapital,
    decimal PrelimOtherCapital,
    // Status
    Guid StatusId,
    Guid? NationalTargetProgramId,
    string? TreasuryCode,
    // Policy decision (optional at creation)
    string? PolicyDecisionNumber,
    DateTime? PolicyDecisionDate,
    string? PolicyDecisionAuthority,
    string? PolicyDecisionPerson,
    Guid? PolicyDecisionFileId
) : ICommand<Guid>;
