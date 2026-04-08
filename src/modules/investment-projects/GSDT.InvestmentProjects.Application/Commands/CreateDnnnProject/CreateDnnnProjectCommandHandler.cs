namespace GSDT.InvestmentProjects.Application.Commands.CreateDnnnProject;

/// <summary>
/// Creates a new DnnnProject aggregate, validates code uniqueness, and persists via repository.
/// TenantId is resolved from ambient ITenantContext.
/// </summary>
public sealed class CreateDnnnProjectCommandHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext)
    : IRequestHandler<CreateDnnnProjectCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateDnnnProjectCommand request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail<Guid>("GOV_INV_401: Tenant context is required.");

        var codeExists = await repository.ProjectCodeExistsAsync(
            request.ProjectCode, tenantId, excludeId: null, ct: cancellationToken);
        if (codeExists)
            return Result.Fail<Guid>($"GOV_INV_002: Ma du an '{request.ProjectCode}' da ton tai trong he thong.");

        var project = DnnnProject.Create(
            tenantId,
            request.ProjectCode,
            request.ProjectName,
            request.ManagingAuthorityId,
            request.IndustrySectorId,
            request.ProjectOwnerId,
            request.ProjectGroupId,
            request.SubProjectType);

        // Apply optional fields
        project.StatusId = request.StatusId;
        project.CompetentAuthorityId = request.CompetentAuthorityId;
        project.InvestorName = request.InvestorName;
        project.StateOwnershipRatio = request.StateOwnershipRatio;
        project.Objective = request.Objective;
        project.PrelimTotalInvestment = request.PrelimTotalInvestment;
        project.PrelimEquityCapital = request.PrelimEquityCapital;
        project.PrelimOdaLoanCapital = request.PrelimOdaLoanCapital;
        project.PrelimCreditLoanCapital = request.PrelimCreditLoanCapital;
        project.AreaHectares = request.AreaHectares;
        project.Capacity = request.Capacity;
        project.MainItems = request.MainItems;
        project.ImplementationTimeline = request.ImplementationTimeline;
        project.ProgressDescription = request.ProgressDescription;
        project.ProjectManagementUnitId = request.ProjectManagementUnitId;
        project.PmuDirectorName = request.PmuDirectorName;
        project.PmuPhone = request.PmuPhone;
        project.PmuEmail = request.PmuEmail;
        project.ImplementationPeriod = request.ImplementationPeriod;
        project.PolicyDecisionNumber = request.PolicyDecisionNumber;
        project.PolicyDecisionDate = request.PolicyDecisionDate;
        project.PolicyDecisionAuthority = request.PolicyDecisionAuthority;
        project.PolicyDecisionPerson = request.PolicyDecisionPerson;
        project.PolicyDecisionFileId = request.PolicyDecisionFileId;

        repository.Add(project);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok(project.Id);
    }
}
