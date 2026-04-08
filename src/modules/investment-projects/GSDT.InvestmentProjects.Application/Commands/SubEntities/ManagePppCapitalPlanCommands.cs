namespace GSDT.InvestmentProjects.Application.Commands.SubEntities;

/// <summary>Adds a state capital allocation plan decision to a PPP project.</summary>
public sealed record AddPppCapitalPlanCommand(
    Guid ProjectId,
    string DecisionType,
    string DecisionNumber,
    DateTime DecisionDate,
    decimal StateCapitalByDecision,
    Guid? FileId,
    string? Notes)
    : IRequest<Result<Guid>>;

/// <summary>Soft-deletes a capital plan record from a PPP project.</summary>
public sealed record DeletePppCapitalPlanCommand(
    Guid ProjectId,
    Guid CapitalPlanId)
    : IRequest<Result>;

public sealed class AddPppCapitalPlanCommandHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext,
    IProjectQueryScopeService scopeService)
    : IRequestHandler<AddPppCapitalPlanCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        AddPppCapitalPlanCommand request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail<Guid>("GOV_INV_401: Tenant context is required.");

        if (!await scopeService.CanModifyProjectAsync(request.ProjectId, cancellationToken))
            return Result.Fail<Guid>(new GSDT.SharedKernel.Errors.ForbiddenError(
                "GOV_INV_403: Ban khong co quyen thao tac tren du an nay."));

        if (string.IsNullOrWhiteSpace(request.DecisionNumber))
            return Result.Fail<Guid>("GOV_INV_VAL: So quyet dinh khong duoc de trong.");

        if (request.StateCapitalByDecision < 0)
            return Result.Fail<Guid>("GOV_INV_VAL: Von nha nuoc theo quyet dinh phai >= 0.");

        var project = await repository.GetPppByIdWithCapitalPlansAsync(request.ProjectId, cancellationToken);
        if (project is null || project.TenantId != tenantId)
            return Result.Fail<Guid>($"GOV_INV_404: Du an PPP khong ton tai (Id={request.ProjectId}).");

        var plan = PppCapitalPlan.Create(
            tenantId, request.ProjectId,
            request.DecisionType.Trim(),
            request.DecisionNumber.Trim(),
            request.DecisionDate,
            request.StateCapitalByDecision,
            request.FileId,
            request.Notes);

        project.CapitalPlans.Add(plan);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok(plan.Id);
    }
}

public sealed class DeletePppCapitalPlanCommandHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext,
    IProjectQueryScopeService scopeService)
    : IRequestHandler<DeletePppCapitalPlanCommand, Result>
{
    public async Task<Result> Handle(
        DeletePppCapitalPlanCommand request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail("GOV_INV_401: Tenant context is required.");

        if (!await scopeService.CanModifyProjectAsync(request.ProjectId, cancellationToken))
            return Result.Fail(new GSDT.SharedKernel.Errors.ForbiddenError(
                "GOV_INV_403: Ban khong co quyen thao tac tren du an nay."));

        var project = await repository.GetPppByIdWithCapitalPlansAsync(request.ProjectId, cancellationToken);
        if (project is null || project.TenantId != tenantId)
            return Result.Fail($"GOV_INV_404: Du an PPP khong ton tai (Id={request.ProjectId}).");

        var plan = project.CapitalPlans.FirstOrDefault(c => c.Id == request.CapitalPlanId);
        if (plan is null)
            return Result.Fail($"GOV_INV_404: Ke hoach von khong ton tai (Id={request.CapitalPlanId}).");

        project.CapitalPlans.Remove(plan);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
