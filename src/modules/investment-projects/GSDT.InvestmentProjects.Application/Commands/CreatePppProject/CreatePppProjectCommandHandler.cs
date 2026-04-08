namespace GSDT.InvestmentProjects.Application.Commands.CreatePppProject;

/// <summary>
/// Creates a new PppProject aggregate, validates code uniqueness, and persists via repository.
/// Capital totals are CLIENT-PROVIDED for PPP (TotalInvestment = StateCapital + EquityCapital + LoanCapital).
/// TenantId is resolved from ambient ITenantContext.
/// </summary>
public sealed class CreatePppProjectCommandHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext)
    : IRequestHandler<CreatePppProjectCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreatePppProjectCommand request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail<Guid>("GOV_INV_401: Tenant context is required.");

        // Enforce project code uniqueness within tenant
        var codeExists = await repository.ProjectCodeExistsAsync(
            request.ProjectCode, tenantId, excludeId: null, ct: cancellationToken);
        if (codeExists)
            return Result.Fail<Guid>($"GOV_INV_002: Ma du an '{request.ProjectCode}' da ton tai trong he thong.");

        var project = PppProject.Create(
            tenantId,
            request.ProjectCode,
            request.ProjectName,
            request.ManagingAuthorityId,
            request.IndustrySectorId,
            request.ProjectOwnerId,
            request.ProjectGroupId,
            request.ContractType,
            request.SubProjectType);

        // Apply optional fields
        project.StatusId = request.StatusId;
        project.CompetentAuthorityId = request.CompetentAuthorityId;
        project.PreparationUnit = request.PreparationUnit;
        project.Objective = request.Objective;
        project.PrelimTotalInvestment = request.PrelimTotalInvestment;
        project.PrelimStateCapital = request.PrelimStateCapital;
        project.PrelimEquityCapital = request.PrelimEquityCapital;
        project.PrelimLoanCapital = request.PrelimLoanCapital;
        project.AreaHectares = request.AreaHectares;
        project.Capacity = request.Capacity;
        project.MainItems = request.MainItems;
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
