namespace GSDT.InvestmentProjects.Application.Commands.SubEntities;

/// <summary>Adds a revenue sharing report to a PPP project.</summary>
public sealed record AddRevenueReportCommand(
    Guid ProjectId,
    int ReportYear,
    string ReportPeriod,
    decimal RevenuePeriod,
    decimal RevenueCumulative,
    decimal? RevenueIncreaseSharing,
    decimal? RevenueDecreaseSharing,
    string? Difficulties,
    string? Recommendations)
    : IRequest<Result<Guid>>;

/// <summary>Updates an existing revenue report on a PPP project.</summary>
public sealed record UpdateRevenueReportCommand(
    Guid ProjectId,
    Guid ReportId,
    int ReportYear,
    string ReportPeriod,
    decimal RevenuePeriod,
    decimal RevenueCumulative,
    decimal? RevenueIncreaseSharing,
    decimal? RevenueDecreaseSharing,
    string? Difficulties,
    string? Recommendations)
    : IRequest<Result>;

/// <summary>Soft-deletes a revenue report from a PPP project.</summary>
public sealed record DeleteRevenueReportCommand(
    Guid ProjectId,
    Guid ReportId)
    : IRequest<Result>;

public sealed class AddRevenueReportCommandHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext,
    IProjectQueryScopeService scopeService)
    : IRequestHandler<AddRevenueReportCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        AddRevenueReportCommand request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail<Guid>("GOV_INV_401: Tenant context is required.");

        if (!await scopeService.CanModifyProjectAsync(request.ProjectId, cancellationToken))
            return Result.Fail<Guid>(new GSDT.SharedKernel.Errors.ForbiddenError(
                "GOV_INV_403: Ban khong co quyen thao tac tren du an nay."));

        if (string.IsNullOrWhiteSpace(request.ReportPeriod))
            return Result.Fail<Guid>("GOV_INV_VAL: Ky bao cao khong duoc de trong.");

        if (request.ReportYear < 2000 || request.ReportYear > 2100)
            return Result.Fail<Guid>("GOV_INV_VAL: Nam bao cao khong hop le.");

        var project = await repository.GetPppByIdWithRevenueReportsAsync(request.ProjectId, cancellationToken);
        if (project is null || project.TenantId != tenantId)
            return Result.Fail<Guid>($"GOV_INV_404: Du an PPP khong ton tai (Id={request.ProjectId}).");

        var report = RevenueReport.Create(
            tenantId, request.ProjectId,
            request.ReportYear, request.ReportPeriod.Trim(),
            request.RevenuePeriod, request.RevenueCumulative,
            request.RevenueIncreaseSharing, request.RevenueDecreaseSharing,
            request.Difficulties, request.Recommendations);

        project.RevenueReports.Add(report);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok(report.Id);
    }
}

public sealed class UpdateRevenueReportCommandHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext,
    IProjectQueryScopeService scopeService)
    : IRequestHandler<UpdateRevenueReportCommand, Result>
{
    public async Task<Result> Handle(
        UpdateRevenueReportCommand request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail("GOV_INV_401: Tenant context is required.");

        if (!await scopeService.CanModifyProjectAsync(request.ProjectId, cancellationToken))
            return Result.Fail(new GSDT.SharedKernel.Errors.ForbiddenError(
                "GOV_INV_403: Ban khong co quyen thao tac tren du an nay."));

        if (string.IsNullOrWhiteSpace(request.ReportPeriod))
            return Result.Fail("GOV_INV_VAL: Ky bao cao khong duoc de trong.");

        var project = await repository.GetPppByIdWithRevenueReportsAsync(request.ProjectId, cancellationToken);
        if (project is null || project.TenantId != tenantId)
            return Result.Fail($"GOV_INV_404: Du an PPP khong ton tai (Id={request.ProjectId}).");

        var report = project.RevenueReports.FirstOrDefault(r => r.Id == request.ReportId);
        if (report is null)
            return Result.Fail($"GOV_INV_404: Bao cao doanh thu khong ton tai (Id={request.ReportId}).");

        report.ReportYear = request.ReportYear;
        report.ReportPeriod = request.ReportPeriod.Trim();
        report.RevenuePeriod = request.RevenuePeriod;
        report.RevenueCumulative = request.RevenueCumulative;
        report.RevenueIncreaseSharing = request.RevenueIncreaseSharing;
        report.RevenueDecreaseSharing = request.RevenueDecreaseSharing;
        report.Difficulties = request.Difficulties;
        report.Recommendations = request.Recommendations;

        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}

public sealed class DeleteRevenueReportCommandHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext,
    IProjectQueryScopeService scopeService)
    : IRequestHandler<DeleteRevenueReportCommand, Result>
{
    public async Task<Result> Handle(
        DeleteRevenueReportCommand request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail("GOV_INV_401: Tenant context is required.");

        if (!await scopeService.CanModifyProjectAsync(request.ProjectId, cancellationToken))
            return Result.Fail(new GSDT.SharedKernel.Errors.ForbiddenError(
                "GOV_INV_403: Ban khong co quyen thao tac tren du an nay."));

        var project = await repository.GetPppByIdWithRevenueReportsAsync(request.ProjectId, cancellationToken);
        if (project is null || project.TenantId != tenantId)
            return Result.Fail($"GOV_INV_404: Du an PPP khong ton tai (Id={request.ProjectId}).");

        var report = project.RevenueReports.FirstOrDefault(r => r.Id == request.ReportId);
        if (report is null)
            return Result.Fail($"GOV_INV_404: Bao cao doanh thu khong ton tai (Id={request.ReportId}).");

        project.RevenueReports.Remove(report);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
