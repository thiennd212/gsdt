namespace GSDT.InvestmentProjects.Application.Commands.SubEntities;

/// <summary>Adds a bid package to an existing investment project.</summary>
public sealed record AddBidPackageCommand(
    Guid ProjectId,
    string Name,
    Guid? ContractorSelectionPlanId,
    bool IsDesignReview,
    bool IsSupervision,
    Guid BidSelectionFormId,
    Guid BidSelectionMethodId,
    Guid ContractFormId,
    Guid BidSectorTypeId,
    int? Duration,
    int? DurationUnit,
    decimal? EstimatedPrice,
    string? Notes)
    : IRequest<Result<Guid>>;

/// <summary>Soft-deletes a bid package from an investment project.</summary>
public sealed record DeleteBidPackageCommand(
    Guid ProjectId,
    Guid BidPackageId)
    : IRequest<Result>;

public sealed class AddBidPackageCommandHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext,
    Services.IProjectQueryScopeService scopeService)
    : IRequestHandler<AddBidPackageCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        AddBidPackageCommand request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail<Guid>("GOV_INV_401: Tenant context is required.");

        if (!await scopeService.CanModifyProjectAsync(request.ProjectId, cancellationToken))
            return Result.Fail<Guid>(new ForbiddenError("GOV_INV_403: Ban khong co quyen thao tac tren du an nay."));

        if (string.IsNullOrWhiteSpace(request.Name))
            return Result.Fail<Guid>("GOV_INV_VAL: Ten goi thau khong duoc de trong.");

        var project = await repository.GetByIdWithBidPackagesAsync(
            request.ProjectId, cancellationToken);
        if (project is null || project.TenantId != tenantId)
            return Result.Fail<Guid>($"GOV_INV_404: Du an khong ton tai (Id={request.ProjectId}).");

        var bidPackage = BidPackage.Create(
            tenantId, request.ProjectId, request.Name.Trim(),
            request.BidSelectionFormId, request.BidSelectionMethodId,
            request.ContractFormId, request.BidSectorTypeId,
            request.ContractorSelectionPlanId,
            request.IsDesignReview, request.IsSupervision,
            request.Duration,
            request.DurationUnit.HasValue ? (TimeUnit)request.DurationUnit.Value : null,
            request.EstimatedPrice,
            request.Notes);

        repository.AddChild(bidPackage);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok(bidPackage.Id);
    }
}

public sealed class DeleteBidPackageCommandHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext,
    Services.IProjectQueryScopeService scopeService)
    : IRequestHandler<DeleteBidPackageCommand, Result>
{
    public async Task<Result> Handle(
        DeleteBidPackageCommand request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail("GOV_INV_401: Tenant context is required.");

        if (!await scopeService.CanModifyProjectAsync(request.ProjectId, cancellationToken))
            return Result.Fail(new ForbiddenError("GOV_INV_403: Ban khong co quyen thao tac tren du an nay."));

        var project = await repository.GetByIdWithBidPackagesAsync(
            request.ProjectId, cancellationToken);
        if (project is null || project.TenantId != tenantId)
            return Result.Fail($"GOV_INV_404: Du an khong ton tai (Id={request.ProjectId}).");

        var bidPackage = project.BidPackages.FirstOrDefault(bp => bp.Id == request.BidPackageId);
        if (bidPackage is null)
            return Result.Fail($"GOV_INV_404: Goi thau khong ton tai (Id={request.BidPackageId}).");

        project.BidPackages.Remove(bidPackage);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
