namespace GSDT.InvestmentProjects.Application.Commands.SubEntities;

/// <summary>Adds a design estimate to a project. SERVER recomputes TotalEstimate from 7 cost fields.</summary>
public sealed record AddDesignEstimateCommand(
    Guid ProjectId,
    decimal EquipmentCost,
    decimal ConstructionCost,
    decimal LandCompensationCost,
    decimal ManagementCost,
    decimal ConsultancyCost,
    decimal ContingencyCost,
    decimal OtherCost,
    string? ApprovalDecisionNumber,
    DateTime? ApprovalDecisionDate,
    string? ApprovalAuthority,
    string? ApprovalSigner,
    string? ApprovalSummary,
    Guid? ApprovalFileId,
    string? Notes)
    : IRequest<Result<Guid>>;

/// <summary>Updates an existing design estimate. SERVER recomputes TotalEstimate — client value ignored.</summary>
public sealed record UpdateDesignEstimateCommand(
    Guid ProjectId,
    Guid EstimateId,
    decimal EquipmentCost,
    decimal ConstructionCost,
    decimal LandCompensationCost,
    decimal ManagementCost,
    decimal ConsultancyCost,
    decimal ContingencyCost,
    decimal OtherCost,
    string? ApprovalDecisionNumber,
    DateTime? ApprovalDecisionDate,
    string? ApprovalAuthority,
    string? ApprovalSigner,
    string? ApprovalSummary,
    Guid? ApprovalFileId,
    string? Notes)
    : IRequest<Result>;

/// <summary>Soft-deletes a design estimate from a project.</summary>
public sealed record DeleteDesignEstimateCommand(
    Guid ProjectId,
    Guid EstimateId)
    : IRequest<Result>;

public sealed class AddDesignEstimateCommandHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext,
    IProjectQueryScopeService scopeService)
    : IRequestHandler<AddDesignEstimateCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        AddDesignEstimateCommand request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail<Guid>("GOV_INV_401: Tenant context is required.");

        if (!await scopeService.CanModifyProjectAsync(request.ProjectId, cancellationToken))
            return Result.Fail<Guid>(new GSDT.SharedKernel.Errors.ForbiddenError(
                "GOV_INV_403: Ban khong co quyen thao tac tren du an nay."));

        var project = await repository.GetByIdWithDesignEstimatesAsync(request.ProjectId, cancellationToken);
        if (project is null || project.TenantId != tenantId)
            return Result.Fail<Guid>($"GOV_INV_404: Du an khong ton tai (Id={request.ProjectId}).");

        // SERVER AUTHORITATIVE: recompute total from the 7 cost fields; ignore any client value
        var total = request.EquipmentCost + request.ConstructionCost + request.LandCompensationCost
                  + request.ManagementCost + request.ConsultancyCost + request.ContingencyCost
                  + request.OtherCost;

        var estimate = DesignEstimate.Create(
            tenantId, request.ProjectId,
            request.EquipmentCost, request.ConstructionCost, request.LandCompensationCost,
            request.ManagementCost, request.ConsultancyCost, request.ContingencyCost,
            request.OtherCost, total, request.Notes);

        estimate.ApprovalDecisionNumber = request.ApprovalDecisionNumber;
        estimate.ApprovalDecisionDate = request.ApprovalDecisionDate;
        estimate.ApprovalAuthority = request.ApprovalAuthority;
        estimate.ApprovalSigner = request.ApprovalSigner;
        estimate.ApprovalSummary = request.ApprovalSummary;
        estimate.ApprovalFileId = request.ApprovalFileId;

        repository.AddChild(estimate);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok(estimate.Id);
    }
}

public sealed class UpdateDesignEstimateCommandHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext,
    IProjectQueryScopeService scopeService)
    : IRequestHandler<UpdateDesignEstimateCommand, Result>
{
    public async Task<Result> Handle(
        UpdateDesignEstimateCommand request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail("GOV_INV_401: Tenant context is required.");

        if (!await scopeService.CanModifyProjectAsync(request.ProjectId, cancellationToken))
            return Result.Fail(new GSDT.SharedKernel.Errors.ForbiddenError(
                "GOV_INV_403: Ban khong co quyen thao tac tren du an nay."));

        var project = await repository.GetByIdWithDesignEstimatesAsync(request.ProjectId, cancellationToken);
        if (project is null || project.TenantId != tenantId)
            return Result.Fail($"GOV_INV_404: Du an khong ton tai (Id={request.ProjectId}).");

        var estimate = project.DesignEstimates.FirstOrDefault(e => e.Id == request.EstimateId);
        if (estimate is null)
            return Result.Fail($"GOV_INV_404: Du toan thiet ke khong ton tai (Id={request.EstimateId}).");

        estimate.EquipmentCost = request.EquipmentCost;
        estimate.ConstructionCost = request.ConstructionCost;
        estimate.LandCompensationCost = request.LandCompensationCost;
        estimate.ManagementCost = request.ManagementCost;
        estimate.ConsultancyCost = request.ConsultancyCost;
        estimate.ContingencyCost = request.ContingencyCost;
        estimate.OtherCost = request.OtherCost;

        // SERVER AUTHORITATIVE: always recompute — client-supplied TotalEstimate is not trusted
        estimate.TotalEstimate = request.EquipmentCost + request.ConstructionCost
            + request.LandCompensationCost + request.ManagementCost + request.ConsultancyCost
            + request.ContingencyCost + request.OtherCost;

        estimate.ApprovalDecisionNumber = request.ApprovalDecisionNumber;
        estimate.ApprovalDecisionDate = request.ApprovalDecisionDate;
        estimate.ApprovalAuthority = request.ApprovalAuthority;
        estimate.ApprovalSigner = request.ApprovalSigner;
        estimate.ApprovalSummary = request.ApprovalSummary;
        estimate.ApprovalFileId = request.ApprovalFileId;
        estimate.Notes = request.Notes;

        await repository.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}

public sealed class DeleteDesignEstimateCommandHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext,
    IProjectQueryScopeService scopeService)
    : IRequestHandler<DeleteDesignEstimateCommand, Result>
{
    public async Task<Result> Handle(
        DeleteDesignEstimateCommand request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail("GOV_INV_401: Tenant context is required.");

        if (!await scopeService.CanModifyProjectAsync(request.ProjectId, cancellationToken))
            return Result.Fail(new GSDT.SharedKernel.Errors.ForbiddenError(
                "GOV_INV_403: Ban khong co quyen thao tac tren du an nay."));

        var project = await repository.GetByIdWithDesignEstimatesAsync(request.ProjectId, cancellationToken);
        if (project is null || project.TenantId != tenantId)
            return Result.Fail($"GOV_INV_404: Du an khong ton tai (Id={request.ProjectId}).");

        var estimate = project.DesignEstimates.FirstOrDefault(e => e.Id == request.EstimateId);
        if (estimate is null)
            return Result.Fail($"GOV_INV_404: Du toan thiet ke khong ton tai (Id={request.EstimateId}).");

        project.DesignEstimates.Remove(estimate);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
