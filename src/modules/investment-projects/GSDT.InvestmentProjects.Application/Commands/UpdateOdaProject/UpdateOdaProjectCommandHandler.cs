using GSDT.SharedKernel.Errors;

namespace GSDT.InvestmentProjects.Application.Commands.UpdateOdaProject;

/// <summary>
/// Updates fields on an existing OdaProject.
/// Checks optimistic concurrency via RowVersion before writing.
/// TotalInvestment is recomputed from capital components.
/// Enforces ownership: CDT can only update their own projects; CQCQ cannot update any project.
/// </summary>
public sealed class UpdateOdaProjectCommandHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext,
    IProjectQueryScopeService scopeService)
    : IRequestHandler<UpdateOdaProjectCommand, Result>
{
    public async Task<Result> Handle(
        UpdateOdaProjectCommand request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail("GOV_INV_401: Tenant context is required.");

        // Ownership check — BTC always passes, CQCQ always fails, CDT only if project owner matches
        var canModify = await scopeService.CanModifyProjectAsync(request.Id, cancellationToken);
        if (!canModify)
            return Result.Fail(new ForbiddenError(
                "GOV_INV_403: Ban khong co quyen cap nhat du an nay."));

        var entity = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Fail($"GOV_INV_001: Du an khong ton tai (Id={request.Id}).");

        if (entity is not OdaProject project)
            return Result.Fail("GOV_INV_004: Du an khong phai loai du an ODA.");

        // Optimistic concurrency check
        if (!project.RowVersion.SequenceEqual(request.RowVersion))
            return Result.Fail("GOV_INV_409: Du lieu da bi thay doi boi nguoi dung khac. Vui long tai lai trang.");

        // Validate code uniqueness when changing code
        if (!string.Equals(project.ProjectCode, request.ProjectCode, StringComparison.OrdinalIgnoreCase))
        {
            var codeExists = await repository.ProjectCodeExistsAsync(
                request.ProjectCode, tenantId, excludeId: request.Id, ct: cancellationToken);
            if (codeExists)
                return Result.Fail($"GOV_INV_002: Ma du an '{request.ProjectCode}' da ton tai trong he thong.");
        }

        // Apply base field updates
        project.ProjectCode = request.ProjectCode;
        project.ProjectName = request.ProjectName;
        project.ManagingAuthorityId = request.ManagingAuthorityId;
        project.IndustrySectorId = request.IndustrySectorId;
        project.ProjectOwnerId = request.ProjectOwnerId;
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

        // Apply ODA-specific field updates
        project.ShortName = request.ShortName;
        project.ProjectCodeQhns = request.ProjectCodeQhns;
        project.OdaProjectTypeId = request.OdaProjectTypeId;
        project.DonorId = request.DonorId;
        project.CoDonorName = request.CoDonorName;
        project.OdaGrantCapital = request.OdaGrantCapital;
        project.OdaLoanCapital = request.OdaLoanCapital;
        project.CounterpartCentralBudget = request.CounterpartCentralBudget;
        project.CounterpartLocalBudget = request.CounterpartLocalBudget;
        project.CounterpartOtherCapital = request.CounterpartOtherCapital;
        // Recompute TotalInvestment = ODA (grant + loan) + counterpart (central + local + other)
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

        await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
