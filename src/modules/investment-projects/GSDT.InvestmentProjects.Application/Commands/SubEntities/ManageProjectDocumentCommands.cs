namespace GSDT.InvestmentProjects.Application.Commands.SubEntities;

/// <summary>Attaches a document to an existing investment project.</summary>
public sealed record AddProjectDocumentCommand(
    Guid ProjectId,
    Guid DocumentTypeId,
    Guid FileId,
    string Title,
    string? Notes)
    : IRequest<Result<Guid>>;

/// <summary>Removes a document attachment from an investment project.</summary>
public sealed record DeleteProjectDocumentCommand(
    Guid ProjectId,
    Guid DocumentId)
    : IRequest<Result>;

public sealed class AddProjectDocumentCommandHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext,
    Services.IProjectQueryScopeService scopeService)
    : IRequestHandler<AddProjectDocumentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        AddProjectDocumentCommand request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail<Guid>("GOV_INV_401: Tenant context is required.");

        if (!await scopeService.CanModifyProjectAsync(request.ProjectId, cancellationToken))
            return Result.Fail<Guid>(new ForbiddenError("GOV_INV_403: Ban khong co quyen thao tac tren du an nay."));

        if (string.IsNullOrWhiteSpace(request.Title))
            return Result.Fail<Guid>("GOV_INV_VAL: Tieu de tai lieu khong duoc de trong.");

        var project = await repository.GetByIdWithDocumentsAsync(
            request.ProjectId, cancellationToken);
        if (project is null || project.TenantId != tenantId)
            return Result.Fail<Guid>($"GOV_INV_404: Du an khong ton tai (Id={request.ProjectId}).");

        var document = ProjectDocument.Create(
            tenantId, request.ProjectId,
            request.DocumentTypeId, request.FileId,
            request.Title.Trim(),
            request.Notes);

        project.Documents.Add(document);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok(document.Id);
    }
}

public sealed class DeleteProjectDocumentCommandHandler(
    IInvestmentProjectRepository repository,
    ITenantContext tenantContext,
    Services.IProjectQueryScopeService scopeService)
    : IRequestHandler<DeleteProjectDocumentCommand, Result>
{
    public async Task<Result> Handle(
        DeleteProjectDocumentCommand request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail("GOV_INV_401: Tenant context is required.");

        if (!await scopeService.CanModifyProjectAsync(request.ProjectId, cancellationToken))
            return Result.Fail(new ForbiddenError("GOV_INV_403: Ban khong co quyen thao tac tren du an nay."));

        var project = await repository.GetByIdWithDocumentsAsync(
            request.ProjectId, cancellationToken);
        if (project is null || project.TenantId != tenantId)
            return Result.Fail($"GOV_INV_404: Du an khong ton tai (Id={request.ProjectId}).");

        var document = project.Documents.FirstOrDefault(d => d.Id == request.DocumentId);
        if (document is null)
            return Result.Fail($"GOV_INV_404: Tai lieu khong ton tai (Id={request.DocumentId}).");

        project.Documents.Remove(document);
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
