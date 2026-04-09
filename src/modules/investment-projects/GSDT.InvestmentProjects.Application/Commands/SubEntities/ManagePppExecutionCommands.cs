namespace GSDT.InvestmentProjects.Application.Commands.SubEntities;

/// <summary>Adds a physical execution progress snapshot to a PPP project.</summary>
public sealed record AddPppExecutionCommand(
    Guid ProjectId,
    DateTime ReportDate,
    decimal ValueExecutedPeriod,
    decimal ValueExecutedCumulative,
    decimal CumulativeFromStart,
    decimal? SubProjectStateCapitalPeriod,
    decimal? SubProjectStateCapitalCumulative,
    Guid? BidPackageId,
    Guid? ContractId)
    : IRequest<Result<Guid>>;

/// <summary>Soft-deletes an execution record from a PPP project.</summary>
public sealed record DeletePppExecutionCommand(
    Guid ProjectId,
    Guid ExecutionId)
    : IRequest<Result>;

public sealed class AddPppExecutionCommandHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext,
    IProjectQueryScopeService scopeService)
    : IRequestHandler<AddPppExecutionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        AddPppExecutionCommand request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail<Guid>("GOV_INV_401: Tenant context is required.");

        if (!await scopeService.CanModifyProjectAsync(request.ProjectId, cancellationToken))
            return Result.Fail<Guid>(new GSDT.SharedKernel.Errors.ForbiddenError(
                "GOV_INV_403: Ban khong co quyen thao tac tren du an nay."));

        var project = await repository.GetPppByIdWithExecutionsAsync(request.ProjectId, cancellationToken);
        if (project is null || project.TenantId != tenantId)
            return Result.Fail<Guid>($"GOV_INV_404: Du an PPP khong ton tai (Id={request.ProjectId}).");

        var record = PppExecutionRecord.Create(
            tenantId, request.ProjectId, request.ReportDate,
            request.ValueExecutedPeriod, request.ValueExecutedCumulative,
            request.CumulativeFromStart,
            request.SubProjectStateCapitalPeriod,
            request.SubProjectStateCapitalCumulative,
            request.BidPackageId,
            request.ContractId);

        repository.AddChild(record);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok(record.Id);
    }
}

public sealed class DeletePppExecutionCommandHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext,
    IProjectQueryScopeService scopeService)
    : IRequestHandler<DeletePppExecutionCommand, Result>
{
    public async Task<Result> Handle(
        DeletePppExecutionCommand request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail("GOV_INV_401: Tenant context is required.");

        if (!await scopeService.CanModifyProjectAsync(request.ProjectId, cancellationToken))
            return Result.Fail(new GSDT.SharedKernel.Errors.ForbiddenError(
                "GOV_INV_403: Ban khong co quyen thao tac tren du an nay."));

        var project = await repository.GetPppByIdWithExecutionsAsync(request.ProjectId, cancellationToken);
        if (project is null || project.TenantId != tenantId)
            return Result.Fail($"GOV_INV_404: Du an PPP khong ton tai (Id={request.ProjectId}).");

        var record = project.ExecutionRecords.FirstOrDefault(e => e.Id == request.ExecutionId);
        if (record is null)
            return Result.Fail($"GOV_INV_404: Tien do thi cong khong ton tai (Id={request.ExecutionId}).");

        project.ExecutionRecords.Remove(record);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
