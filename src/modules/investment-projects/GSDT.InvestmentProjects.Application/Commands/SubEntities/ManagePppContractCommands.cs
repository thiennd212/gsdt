namespace GSDT.InvestmentProjects.Application.Commands.SubEntities;

// ── Investor Selection ────────────────────────────────────────────────────────

/// <summary>
/// Upserts the InvestorSelection record for a project (create or replace).
/// Also replaces the full InvestorSelectionInvestor junction list.
/// </summary>
public sealed record UpsertInvestorSelectionCommand(
    Guid ProjectId,
    string? SelectionMethod,
    string? SelectionDecisionNumber,
    DateTime? SelectionDecisionDate,
    Guid? SelectionFileId,
    IReadOnlyList<InvestorSelectionInvestorRequest> Investors)
    : IRequest<Result>;

/// <summary>One investor entry in the UpsertInvestorSelection request.</summary>
public sealed record InvestorSelectionInvestorRequest(Guid InvestorId, int SortOrder);

public sealed class UpsertInvestorSelectionCommandHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext,
    IProjectQueryScopeService scopeService)
    : IRequestHandler<UpsertInvestorSelectionCommand, Result>
{
    public async Task<Result> Handle(
        UpsertInvestorSelectionCommand request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail("GOV_INV_401: Tenant context is required.");

        if (!await scopeService.CanModifyProjectAsync(request.ProjectId, cancellationToken))
            return Result.Fail(new GSDT.SharedKernel.Errors.ForbiddenError(
                "GOV_INV_403: Ban khong co quyen thao tac tren du an nay."));

        var project = await repository.GetByIdWithInvestorSelectionAsync(request.ProjectId, cancellationToken);
        if (project is null || project.TenantId != tenantId)
            return Result.Fail($"GOV_INV_404: Du an khong ton tai (Id={request.ProjectId}).");

        if (project.InvestorSelection is null)
        {
            // Create new
            var selection = InvestorSelection.Create(
                request.ProjectId, tenantId,
                request.SelectionMethod,
                request.SelectionDecisionNumber,
                request.SelectionDecisionDate,
                request.SelectionFileId);

            foreach (var inv in request.Investors)
                selection.Investors.Add(InvestorSelectionInvestor.Create(
                    selection.ProjectId, inv.InvestorId, inv.SortOrder));

            project.InvestorSelection = selection;
        }
        else
        {
            // Update existing
            var sel = project.InvestorSelection;
            sel.SelectionMethod = request.SelectionMethod;
            sel.SelectionDecisionNumber = request.SelectionDecisionNumber;
            sel.SelectionDecisionDate = request.SelectionDecisionDate;
            sel.SelectionFileId = request.SelectionFileId;

            // Replace investor list entirely
            sel.Investors.Clear();
            foreach (var inv in request.Investors)
                sel.Investors.Add(InvestorSelectionInvestor.Create(
                    sel.ProjectId, inv.InvestorId, inv.SortOrder));
        }

        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}

// ── PPP Contract Info ─────────────────────────────────────────────────────────

/// <summary>
/// Upserts the PppContractInfo record for a PPP project (create or replace).
/// Validates: TotalInvestment == StateCapital + EquityCapital + LoanCapital
/// and StateCapital == CentralBudget + LocalBudget + OtherStateBudget.
/// </summary>
public sealed record UpsertPppContractInfoCommand(
    Guid ProjectId,
    decimal TotalInvestment,
    decimal StateCapital,
    decimal CentralBudget,
    decimal LocalBudget,
    decimal OtherStateBudget,
    decimal EquityCapital,
    decimal LoanCapital,
    decimal? EquityRatio,
    string? ImplementationProgress,
    string? ContractDuration,
    string? RevenueSharingMechanism,
    string? ContractAuthority,
    string? ContractNumber,
    DateTime? ContractDate,
    DateTime? ConstructionStartDate,
    DateTime? CompletionDate)
    : IRequest<Result>;

public sealed class UpsertPppContractInfoCommandHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext,
    IProjectQueryScopeService scopeService)
    : IRequestHandler<UpsertPppContractInfoCommand, Result>
{
    public async Task<Result> Handle(
        UpsertPppContractInfoCommand request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail("GOV_INV_401: Tenant context is required.");

        if (!await scopeService.CanModifyProjectAsync(request.ProjectId, cancellationToken))
            return Result.Fail(new GSDT.SharedKernel.Errors.ForbiddenError(
                "GOV_INV_403: Ban khong co quyen thao tac tren du an nay."));

        // VALIDATE: TotalInvestment == StateCapital + EquityCapital + LoanCapital
        var expectedTotal = request.StateCapital + request.EquityCapital + request.LoanCapital;
        if (request.TotalInvestment != expectedTotal)
            return Result.Fail(
                "GOV_INV_VAL: Tong von dau tu phai bang von nha nuoc + von chu so huu + von vay.");

        // VALIDATE: StateCapital == CentralBudget + LocalBudget + OtherStateBudget
        var expectedState = request.CentralBudget + request.LocalBudget + request.OtherStateBudget;
        if (request.StateCapital != expectedState)
            return Result.Fail(
                "GOV_INV_VAL: Von nha nuoc phai bang von TW + von dia phuong + von nha nuoc khac.");

        var project = await repository.GetPppByIdWithContractInfoAsync(request.ProjectId, cancellationToken);
        if (project is null || project.TenantId != tenantId)
            return Result.Fail($"GOV_INV_404: Du an PPP khong ton tai (Id={request.ProjectId}).");

        if (project.ContractInfo is null)
        {
            project.ContractInfo = PppContractInfo.Create(
                request.ProjectId, tenantId,
                request.TotalInvestment, request.StateCapital,
                request.CentralBudget, request.LocalBudget,
                request.OtherStateBudget, request.EquityCapital, request.LoanCapital);
        }
        else
        {
            var ci = project.ContractInfo;
            ci.TotalInvestment = request.TotalInvestment;
            ci.StateCapital = request.StateCapital;
            ci.CentralBudget = request.CentralBudget;
            ci.LocalBudget = request.LocalBudget;
            ci.OtherStateBudget = request.OtherStateBudget;
            ci.EquityCapital = request.EquityCapital;
            ci.LoanCapital = request.LoanCapital;
        }

        // Apply optional fields (always)
        var contractInfo = project.ContractInfo;
        contractInfo.EquityRatio = request.EquityRatio;
        contractInfo.ImplementationProgress = request.ImplementationProgress;
        contractInfo.ContractDuration = request.ContractDuration;
        contractInfo.RevenueSharingMechanism = request.RevenueSharingMechanism;
        contractInfo.ContractAuthority = request.ContractAuthority;
        contractInfo.ContractNumber = request.ContractNumber;
        contractInfo.ContractDate = request.ContractDate;
        contractInfo.ConstructionStartDate = request.ConstructionStartDate;
        contractInfo.CompletionDate = request.CompletionDate;

        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
