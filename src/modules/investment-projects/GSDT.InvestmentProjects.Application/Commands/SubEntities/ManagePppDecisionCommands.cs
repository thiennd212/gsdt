namespace GSDT.InvestmentProjects.Application.Commands.SubEntities;

/// <summary>Adds an investment decision to a PPP project.</summary>
public sealed record AddPppDecisionCommand(
    Guid ProjectId,
    int DecisionType,
    string DecisionNumber,
    DateTime DecisionDate,
    string DecisionAuthority,
    string? DecisionPerson,
    decimal TotalInvestment,
    decimal StateCapital,
    decimal CentralBudget,
    decimal LocalBudget,
    decimal OtherStateBudget,
    decimal EquityCapital,
    decimal LoanCapital,
    decimal? EquityRatio,
    Guid? AdjustmentContentId,
    string? Notes,
    Guid? FileId)
    : IRequest<Result<Guid>>;

/// <summary>Soft-deletes an investment decision from a PPP project.</summary>
public sealed record DeletePppDecisionCommand(
    Guid ProjectId,
    Guid DecisionId)
    : IRequest<Result>;

public sealed class AddPppDecisionCommandHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext,
    IProjectQueryScopeService scopeService)
    : IRequestHandler<AddPppDecisionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        AddPppDecisionCommand request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail<Guid>("GOV_INV_401: Tenant context is required.");

        if (!await scopeService.CanModifyProjectAsync(request.ProjectId, cancellationToken))
            return Result.Fail<Guid>(new GSDT.SharedKernel.Errors.ForbiddenError(
                "GOV_INV_403: Ban khong co quyen thao tac tren du an nay."));

        // VALIDATE: TotalInvestment == StateCapital + EquityCapital + LoanCapital
        var expectedTotal = request.StateCapital + request.EquityCapital + request.LoanCapital;
        if (request.TotalInvestment != expectedTotal)
            return Result.Fail<Guid>(
                "GOV_INV_VAL: Tong von dau tu phai bang von nha nuoc + von chu so huu + von vay.");

        // VALIDATE: StateCapital == CentralBudget + LocalBudget + OtherStateBudget
        var expectedState = request.CentralBudget + request.LocalBudget + request.OtherStateBudget;
        if (request.StateCapital != expectedState)
            return Result.Fail<Guid>(
                "GOV_INV_VAL: Von nha nuoc phai bang von TW + von dia phuong + von nha nuoc khac.");

        if (string.IsNullOrWhiteSpace(request.DecisionNumber))
            return Result.Fail<Guid>("GOV_INV_VAL: So quyet dinh khong duoc de trong.");

        if (string.IsNullOrWhiteSpace(request.DecisionAuthority))
            return Result.Fail<Guid>("GOV_INV_VAL: Co quan quyet dinh khong duoc de trong.");

        var project = await repository.GetPppByIdWithDecisionsAsync(request.ProjectId, cancellationToken);
        if (project is null || project.TenantId != tenantId)
            return Result.Fail<Guid>($"GOV_INV_404: Du an PPP khong ton tai (Id={request.ProjectId}).");

        var decision = PppInvestmentDecision.Create(
            tenantId, request.ProjectId,
            (InvestmentDecisionType)request.DecisionType,
            request.DecisionNumber.Trim(),
            request.DecisionDate,
            request.DecisionAuthority.Trim(),
            request.TotalInvestment,
            request.StateCapital,
            request.CentralBudget,
            request.LocalBudget,
            request.OtherStateBudget,
            request.EquityCapital,
            request.LoanCapital,
            request.DecisionPerson,
            request.EquityRatio,
            request.AdjustmentContentId,
            request.Notes,
            request.FileId);

        project.InvestmentDecisions.Add(decision);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok(decision.Id);
    }
}

public sealed class DeletePppDecisionCommandHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext,
    IProjectQueryScopeService scopeService)
    : IRequestHandler<DeletePppDecisionCommand, Result>
{
    public async Task<Result> Handle(
        DeletePppDecisionCommand request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail("GOV_INV_401: Tenant context is required.");

        if (!await scopeService.CanModifyProjectAsync(request.ProjectId, cancellationToken))
            return Result.Fail(new GSDT.SharedKernel.Errors.ForbiddenError(
                "GOV_INV_403: Ban khong co quyen thao tac tren du an nay."));

        var project = await repository.GetPppByIdWithDecisionsAsync(request.ProjectId, cancellationToken);
        if (project is null || project.TenantId != tenantId)
            return Result.Fail($"GOV_INV_404: Du an PPP khong ton tai (Id={request.ProjectId}).");

        var decision = project.InvestmentDecisions.FirstOrDefault(d => d.Id == request.DecisionId);
        if (decision is null)
            return Result.Fail($"GOV_INV_404: Quyet dinh khong ton tai (Id={request.DecisionId}).");

        project.InvestmentDecisions.Remove(decision);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
