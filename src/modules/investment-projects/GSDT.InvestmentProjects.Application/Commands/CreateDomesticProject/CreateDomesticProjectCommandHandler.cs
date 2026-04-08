namespace GSDT.InvestmentProjects.Application.Commands.CreateDomesticProject;

/// <summary>
/// Creates a new DomesticProject aggregate, validates uniqueness, and persists via repository.
/// TenantId is resolved from ambient ITenantContext — must be present on every authenticated request.
/// </summary>
public sealed class CreateDomesticProjectCommandHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext)
    : IRequestHandler<CreateDomesticProjectCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateDomesticProjectCommand request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail<Guid>("GOV_INV_401: Tenant context is required.");

        // Enforce project code uniqueness within tenant
        var codeExists = await repository.ProjectCodeExistsAsync(
            request.ProjectCode, tenantId, excludeId: null, ct: cancellationToken);
        if (codeExists)
            return Result.Fail<Guid>($"GOV_INV_002: Ma du an '{request.ProjectCode}' da ton tai trong he thong.");

        var project = DomesticProject.Create(
            tenantId,
            request.ProjectCode,
            request.ProjectName,
            request.ManagingAuthorityId,
            request.IndustrySectorId,
            request.ProjectOwnerId,
            request.ProjectGroupId,
            request.SubProjectType);

        // Apply optional fields that are not part of the factory signature
        project.ProjectManagementUnitId = request.ProjectManagementUnitId;
        project.PmuDirectorName = request.PmuDirectorName;
        project.PmuPhone = request.PmuPhone;
        project.PmuEmail = request.PmuEmail;
        project.ImplementationPeriod = request.ImplementationPeriod;
        project.TreasuryCode = request.TreasuryCode;
        project.StatusId = request.StatusId;
        project.NationalTargetProgramId = request.NationalTargetProgramId;
        project.PolicyDecisionNumber = request.PolicyDecisionNumber;
        project.PolicyDecisionDate = request.PolicyDecisionDate;
        project.PolicyDecisionAuthority = request.PolicyDecisionAuthority;
        project.PolicyDecisionPerson = request.PolicyDecisionPerson;
        project.PolicyDecisionFileId = request.PolicyDecisionFileId;

        // Computed capital fields: PrelimPublicInvestment = Central + Local + OtherPublic
        project.PrelimCentralBudget = request.PrelimCentralBudget;
        project.PrelimLocalBudget = request.PrelimLocalBudget;
        project.PrelimOtherPublicCapital = request.PrelimOtherPublicCapital;
        project.PrelimPublicInvestment =
            request.PrelimCentralBudget + request.PrelimLocalBudget + request.PrelimOtherPublicCapital;
        project.PrelimOtherCapital = request.PrelimOtherCapital;
        project.PrelimTotalInvestment = project.PrelimPublicInvestment + request.PrelimOtherCapital;

        repository.Add(project);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok(project.Id);
    }
}
