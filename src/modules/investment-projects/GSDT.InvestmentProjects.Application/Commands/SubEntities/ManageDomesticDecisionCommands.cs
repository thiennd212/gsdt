namespace GSDT.InvestmentProjects.Application.Commands.SubEntities;

/// <summary>Adds an investment decision record to a domestic project.</summary>
public sealed record AddDomesticDecisionCommand(
    Guid ProjectId,
    int DecisionType,
    string DecisionNumber,
    DateTime DecisionDate,
    string DecisionAuthority,
    decimal TotalInvestment,
    decimal CentralBudget,
    decimal LocalBudget,
    decimal OtherPublicCapital,
    decimal OtherCapital,
    Guid? AdjustmentContentId,
    string? Notes,
    Guid? FileId)
    : IRequest<Result<Guid>>;

/// <summary>Soft-deletes an investment decision record from a domestic project.</summary>
public sealed record DeleteDomesticDecisionCommand(
    Guid ProjectId,
    Guid DecisionId)
    : IRequest<Result>;

public sealed class AddDomesticDecisionCommandHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext,
    Services.IProjectQueryScopeService scopeService)
    : IRequestHandler<AddDomesticDecisionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        AddDomesticDecisionCommand request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail<Guid>("GOV_INV_401: Tenant context is required.");

        if (!await scopeService.CanModifyProjectAsync(request.ProjectId, cancellationToken))
            return Result.Fail<Guid>(new ForbiddenError("GOV_INV_403: Ban khong co quyen thao tac tren du an nay."));

        if (string.IsNullOrWhiteSpace(request.DecisionNumber))
            return Result.Fail<Guid>("GOV_INV_VAL: So quyet dinh khong duoc de trong.");

        if (string.IsNullOrWhiteSpace(request.DecisionAuthority))
            return Result.Fail<Guid>("GOV_INV_VAL: Co quan quyet dinh khong duoc de trong.");

        var project = await repository.GetDomesticByIdWithDecisionsAsync(
            request.ProjectId, cancellationToken);
        if (project is null || project.TenantId != tenantId)
            return Result.Fail<Guid>($"GOV_INV_404: Du an khong ton tai (Id={request.ProjectId}).");

        var decision = DomesticInvestmentDecision.Create(
            tenantId, request.ProjectId,
            (InvestmentDecisionType)request.DecisionType,
            request.DecisionNumber.Trim(),
            request.DecisionDate,
            request.DecisionAuthority.Trim(),
            request.TotalInvestment,
            request.CentralBudget,
            request.LocalBudget,
            request.OtherPublicCapital,
            request.OtherCapital,
            request.AdjustmentContentId,
            request.Notes,
            request.FileId);

        repository.AddChild(decision);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok(decision.Id);
    }
}

public sealed class DeleteDomesticDecisionCommandHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext,
    Services.IProjectQueryScopeService scopeService)
    : IRequestHandler<DeleteDomesticDecisionCommand, Result>
{
    public async Task<Result> Handle(
        DeleteDomesticDecisionCommand request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail("GOV_INV_401: Tenant context is required.");

        if (!await scopeService.CanModifyProjectAsync(request.ProjectId, cancellationToken))
            return Result.Fail(new ForbiddenError("GOV_INV_403: Ban khong co quyen thao tac tren du an nay."));

        var project = await repository.GetDomesticByIdWithDecisionsAsync(
            request.ProjectId, cancellationToken);
        if (project is null || project.TenantId != tenantId)
            return Result.Fail($"GOV_INV_404: Du an khong ton tai (Id={request.ProjectId}).");

        var decision = project.InvestmentDecisions.FirstOrDefault(d => d.Id == request.DecisionId);
        if (decision is null)
            return Result.Fail($"GOV_INV_404: Quyet dinh khong ton tai (Id={request.DecisionId}).");

        project.InvestmentDecisions.Remove(decision);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
