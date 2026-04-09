namespace GSDT.InvestmentProjects.Application.Commands.SubEntities;

/// <summary>Adds an investment decision to a DNNN project.</summary>
public sealed record AddDnnnDecisionCommand(
    Guid ProjectId,
    int DecisionType,
    string DecisionNumber,
    DateTime DecisionDate,
    string DecisionAuthority,
    string? DecisionPerson,
    decimal TotalInvestment,
    decimal EquityCapital,
    decimal OdaLoanCapital,
    decimal CreditLoanCapital,
    decimal? EquityRatio,
    Guid? AdjustmentContentId,
    string? Notes,
    Guid? FileId)
    : IRequest<Result<Guid>>;

/// <summary>Soft-deletes an investment decision from a DNNN project.</summary>
public sealed record DeleteDnnnDecisionCommand(
    Guid ProjectId,
    Guid DecisionId)
    : IRequest<Result>;

public sealed class AddDnnnDecisionCommandHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext,
    IProjectQueryScopeService scopeService)
    : IRequestHandler<AddDnnnDecisionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        AddDnnnDecisionCommand request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail<Guid>("GOV_INV_401: Tenant context is required.");

        if (!await scopeService.CanModifyProjectAsync(request.ProjectId, cancellationToken))
            return Result.Fail<Guid>(new GSDT.SharedKernel.Errors.ForbiddenError(
                "GOV_INV_403: Ban khong co quyen thao tac tren du an nay."));

        // VALIDATE: TotalInvestment == EquityCapital + OdaLoanCapital + CreditLoanCapital
        var expectedTotal = request.EquityCapital + request.OdaLoanCapital + request.CreditLoanCapital;
        if (request.TotalInvestment != expectedTotal)
            return Result.Fail<Guid>(
                "GOV_INV_VAL: Tong von dau tu phai bang von CSH + von vay ODA + von vay TCTD.");

        if (string.IsNullOrWhiteSpace(request.DecisionNumber))
            return Result.Fail<Guid>("GOV_INV_VAL: So quyet dinh khong duoc de trong.");

        if (string.IsNullOrWhiteSpace(request.DecisionAuthority))
            return Result.Fail<Guid>("GOV_INV_VAL: Co quan quyet dinh khong duoc de trong.");

        var project = await repository.GetDnnnByIdWithDecisionsAsync(request.ProjectId, cancellationToken);
        if (project is null || project.TenantId != tenantId)
            return Result.Fail<Guid>($"GOV_INV_404: Du an DNNN khong ton tai (Id={request.ProjectId}).");

        var decision = DnnnInvestmentDecision.Create(
            tenantId, request.ProjectId,
            (InvestmentDecisionType)request.DecisionType,
            request.DecisionNumber.Trim(),
            request.DecisionDate,
            request.DecisionAuthority.Trim(),
            request.TotalInvestment,
            request.EquityCapital,
            request.OdaLoanCapital,
            request.CreditLoanCapital,
            request.DecisionPerson,
            request.EquityRatio,
            request.AdjustmentContentId,
            request.Notes,
            request.FileId);

        repository.AddChild(decision);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok(decision.Id);
    }
}

public sealed class DeleteDnnnDecisionCommandHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext,
    IProjectQueryScopeService scopeService)
    : IRequestHandler<DeleteDnnnDecisionCommand, Result>
{
    public async Task<Result> Handle(
        DeleteDnnnDecisionCommand request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail("GOV_INV_401: Tenant context is required.");

        if (!await scopeService.CanModifyProjectAsync(request.ProjectId, cancellationToken))
            return Result.Fail(new GSDT.SharedKernel.Errors.ForbiddenError(
                "GOV_INV_403: Ban khong co quyen thao tac tren du an nay."));

        var project = await repository.GetDnnnByIdWithDecisionsAsync(request.ProjectId, cancellationToken);
        if (project is null || project.TenantId != tenantId)
            return Result.Fail($"GOV_INV_404: Du an DNNN khong ton tai (Id={request.ProjectId}).");

        var decision = project.InvestmentDecisions.FirstOrDefault(d => d.Id == request.DecisionId);
        if (decision is null)
            return Result.Fail($"GOV_INV_404: Quyet dinh khong ton tai (Id={request.DecisionId}).");

        project.InvestmentDecisions.Remove(decision);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
