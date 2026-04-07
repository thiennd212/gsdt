namespace GSDT.InvestmentProjects.Application.Commands.UpdateDomesticProject;

/// <summary>Updates an existing domestic project. RowVersion enforces optimistic concurrency.</summary>
public sealed record UpdateDomesticProjectCommand(
    Guid Id,
    byte[] RowVersion,
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
    // Stop/suspension
    string? StopContent,
    string? StopDecisionNumber,
    DateTime? StopDecisionDate,
    Guid? StopFileId,
    // Policy decision
    string? PolicyDecisionNumber,
    DateTime? PolicyDecisionDate,
    string? PolicyDecisionAuthority,
    string? PolicyDecisionPerson,
    Guid? PolicyDecisionFileId
) : ICommand;
