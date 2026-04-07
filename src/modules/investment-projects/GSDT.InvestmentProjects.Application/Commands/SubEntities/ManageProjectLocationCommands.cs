namespace GSDT.InvestmentProjects.Application.Commands.SubEntities;

/// <summary>Adds a geographic location to an existing investment project.</summary>
public sealed record AddProjectLocationCommand(
    Guid ProjectId,
    Guid ProvinceId,
    Guid? DistrictId,
    Guid? WardId,
    string? Address)
    : IRequest<Result<Guid>>;

/// <summary>Soft-deletes a location entry from an investment project.</summary>
public sealed record DeleteProjectLocationCommand(
    Guid ProjectId,
    Guid LocationId)
    : IRequest<Result>;

public sealed class AddProjectLocationCommandHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext,
    Services.IProjectQueryScopeService scopeService)
    : IRequestHandler<AddProjectLocationCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        AddProjectLocationCommand request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail<Guid>("GOV_INV_401: Tenant context is required.");

        if (!await scopeService.CanModifyProjectAsync(request.ProjectId, cancellationToken))
            return Result.Fail<Guid>(new ForbiddenError("GOV_INV_403: Ban khong co quyen thao tac tren du an nay."));

        var project = await repository.GetByIdWithLocationsAsync(
            request.ProjectId, cancellationToken);
        if (project is null || project.TenantId != tenantId)
            return Result.Fail<Guid>($"GOV_INV_404: Du an khong ton tai (Id={request.ProjectId}).");

        var location = ProjectLocation.Create(
            tenantId, request.ProjectId, request.ProvinceId,
            request.DistrictId, request.WardId, request.Address);

        project.Locations.Add(location);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok(location.Id);
    }
}

public sealed class DeleteProjectLocationCommandHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext,
    Services.IProjectQueryScopeService scopeService)
    : IRequestHandler<DeleteProjectLocationCommand, Result>
{
    public async Task<Result> Handle(
        DeleteProjectLocationCommand request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail("GOV_INV_401: Tenant context is required.");

        if (!await scopeService.CanModifyProjectAsync(request.ProjectId, cancellationToken))
            return Result.Fail(new ForbiddenError("GOV_INV_403: Ban khong co quyen thao tac tren du an nay."));

        var project = await repository.GetByIdWithLocationsAsync(
            request.ProjectId, cancellationToken);
        if (project is null || project.TenantId != tenantId)
            return Result.Fail($"GOV_INV_404: Du an khong ton tai (Id={request.ProjectId}).");

        var location = project.Locations.FirstOrDefault(l => l.Id == request.LocationId);
        if (location is null)
            return Result.Fail($"GOV_INV_404: Vi tri khong ton tai (Id={request.LocationId}).");

        project.Locations.Remove(location);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
