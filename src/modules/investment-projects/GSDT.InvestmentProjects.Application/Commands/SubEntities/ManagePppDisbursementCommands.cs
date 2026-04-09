namespace GSDT.InvestmentProjects.Application.Commands.SubEntities;

/// <summary>Adds a disbursement snapshot record to a PPP project.</summary>
public sealed record AddPppDisbursementCommand(
    Guid ProjectId,
    DateTime ReportDate,
    decimal StateCapitalPeriod,
    decimal StateCapitalCumulative,
    decimal EquityCapitalPeriod,
    decimal EquityCapitalCumulative,
    decimal LoanCapitalPeriod,
    decimal LoanCapitalCumulative)
    : IRequest<Result<Guid>>;

/// <summary>Soft-deletes a disbursement record from a PPP project.</summary>
public sealed record DeletePppDisbursementCommand(
    Guid ProjectId,
    Guid DisbursementId)
    : IRequest<Result>;

public sealed class AddPppDisbursementCommandHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext,
    IProjectQueryScopeService scopeService)
    : IRequestHandler<AddPppDisbursementCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        AddPppDisbursementCommand request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail<Guid>("GOV_INV_401: Tenant context is required.");

        if (!await scopeService.CanModifyProjectAsync(request.ProjectId, cancellationToken))
            return Result.Fail<Guid>(new GSDT.SharedKernel.Errors.ForbiddenError(
                "GOV_INV_403: Ban khong co quyen thao tac tren du an nay."));

        var project = await repository.GetPppByIdWithDisbursementsAsync(request.ProjectId, cancellationToken);
        if (project is null || project.TenantId != tenantId)
            return Result.Fail<Guid>($"GOV_INV_404: Du an PPP khong ton tai (Id={request.ProjectId}).");

        var record = PppDisbursementRecord.Create(
            tenantId, request.ProjectId, request.ReportDate,
            request.StateCapitalPeriod, request.StateCapitalCumulative,
            request.EquityCapitalPeriod, request.EquityCapitalCumulative,
            request.LoanCapitalPeriod, request.LoanCapitalCumulative);

        repository.AddChild(record);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok(record.Id);
    }
}

public sealed class DeletePppDisbursementCommandHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext,
    IProjectQueryScopeService scopeService)
    : IRequestHandler<DeletePppDisbursementCommand, Result>
{
    public async Task<Result> Handle(
        DeletePppDisbursementCommand request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail("GOV_INV_401: Tenant context is required.");

        if (!await scopeService.CanModifyProjectAsync(request.ProjectId, cancellationToken))
            return Result.Fail(new GSDT.SharedKernel.Errors.ForbiddenError(
                "GOV_INV_403: Ban khong co quyen thao tac tren du an nay."));

        var project = await repository.GetPppByIdWithDisbursementsAsync(request.ProjectId, cancellationToken);
        if (project is null || project.TenantId != tenantId)
            return Result.Fail($"GOV_INV_404: Du an PPP khong ton tai (Id={request.ProjectId}).");

        var record = project.DisbursementRecords.FirstOrDefault(d => d.Id == request.DisbursementId);
        if (record is null)
            return Result.Fail($"GOV_INV_404: Giai ngan khong ton tai (Id={request.DisbursementId}).");

        project.DisbursementRecords.Remove(record);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
