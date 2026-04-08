namespace GSDT.InvestmentProjects.Application.Commands.CreateOdaProject;

/// <summary>
/// Creates a new OdaProject aggregate, validates uniqueness, and persists via repository.
/// TotalInvestment is computed as sum of ODA + counterpart capital components.
/// </summary>
public sealed class CreateOdaProjectCommandHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext)
    : IRequestHandler<CreateOdaProjectCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateOdaProjectCommand request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail<Guid>("GOV_INV_401: Tenant context is required.");

        var codeExists = await repository.ProjectCodeExistsAsync(
            request.ProjectCode, tenantId, excludeId: null, ct: cancellationToken);
        if (codeExists)
            return Result.Fail<Guid>($"GOV_INV_002: Ma du an '{request.ProjectCode}' da ton tai trong he thong.");

        var project = OdaProject.Create(
            tenantId,
            request.ProjectCode,
            request.ProjectName,
            request.ShortName,
            request.ManagingAuthorityId,
            request.IndustrySectorId,
            request.ProjectOwnerId,
            request.OdaProjectTypeId,
            request.DonorId);

        // Apply optional base fields
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

        // ODA-specific fields
        project.ProjectCodeQhns = request.ProjectCodeQhns;
        project.CoDonorName = request.CoDonorName;
        project.OdaGrantCapital = request.OdaGrantCapital;
        project.OdaLoanCapital = request.OdaLoanCapital;
        project.CounterpartCentralBudget = request.CounterpartCentralBudget;
        project.CounterpartLocalBudget = request.CounterpartLocalBudget;
        project.CounterpartOtherCapital = request.CounterpartOtherCapital;
        // TotalInvestment = ODA (grant + loan) + counterpart (central + local + other)
        project.TotalInvestment =
            request.OdaGrantCapital + request.OdaLoanCapital +
            request.CounterpartCentralBudget + request.CounterpartLocalBudget +
            request.CounterpartOtherCapital;
        project.GrantMechanismPercent = request.GrantMechanismPercent;
        project.RelendingMechanismPercent = request.RelendingMechanismPercent;
        project.StatusId = request.StatusId;
        project.ProcurementConditionBound = request.ProcurementConditionBound;
        project.ProcurementConditionSummary = request.ProcurementConditionSummary;
        project.StartYear = request.StartYear;
        project.EndYear = request.EndYear;

        repository.Add(project);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok(project.Id);
    }
}
