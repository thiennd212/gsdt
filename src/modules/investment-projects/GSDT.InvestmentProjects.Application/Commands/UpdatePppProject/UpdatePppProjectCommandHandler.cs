using GSDT.SharedKernel.Errors;

namespace GSDT.InvestmentProjects.Application.Commands.UpdatePppProject;

/// <summary>
/// Updates fields on an existing PppProject.
/// Checks optimistic concurrency via RowVersion before writing.
/// Enforces ownership: CDT can only update their own projects; CQCQ cannot update any.
/// </summary>
public sealed class UpdatePppProjectCommandHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext,
    IProjectQueryScopeService scopeService)
    : IRequestHandler<UpdatePppProjectCommand, Result>
{
    public async Task<Result> Handle(
        UpdatePppProjectCommand request,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail("GOV_INV_401: Tenant context is required.");

        var canModify = await scopeService.CanModifyProjectAsync(request.Id, cancellationToken);
        if (!canModify)
            return Result.Fail(new ForbiddenError(
                "GOV_INV_403: Ban khong co quyen cap nhat du an nay."));

        var entity = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
            return Result.Fail($"GOV_INV_001: Du an khong ton tai (Id={request.Id}).");

        if (entity is not PppProject project)
            return Result.Fail("GOV_INV_003: Du an khong phai loai du an PPP.");

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

        // Apply updates
        project.ProjectCode = request.ProjectCode;
        project.ProjectName = request.ProjectName;
        project.ManagingAuthorityId = request.ManagingAuthorityId;
        project.IndustrySectorId = request.IndustrySectorId;
        project.ProjectOwnerId = request.ProjectOwnerId;
        project.ProjectGroupId = request.ProjectGroupId;
        project.ContractType = request.ContractType;
        project.SubProjectType = request.SubProjectType;
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
        project.StopContent = request.StopContent;
        project.StopDecisionNumber = request.StopDecisionNumber;
        project.StopDecisionDate = request.StopDecisionDate;
        project.StopFileId = request.StopFileId;
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

        await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
