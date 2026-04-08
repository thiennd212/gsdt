using GSDT.SharedKernel.Errors;

namespace GSDT.InvestmentProjects.Application.Commands.DeleteProject;

/// <summary>
/// Soft-deletes the project aggregate.
/// The SoftDeleteInterceptor in Infrastructure cascades IsDeleted=true to all child tables
/// via the EF Core SaveChanges pipeline — no explicit child enumeration needed here.
/// Enforces ownership: CDT can only delete their own projects; CQCQ cannot delete any project.
/// </summary>
public sealed class DeleteProjectCommandHandler(
    IInvestmentProjectRepository repository,
    IProjectQueryScopeService scopeService)
    : IRequestHandler<DeleteProjectCommand, Result>
{
    public async Task<Result> Handle(
        DeleteProjectCommand request,
        CancellationToken cancellationToken)
    {
        // Ownership check — BTC always passes, CQCQ always fails, CDT only if project owner matches
        var canModify = await scopeService.CanModifyProjectAsync(request.Id, cancellationToken);
        if (!canModify)
            return Result.Fail(new ForbiddenError(
                "GOV_INV_403: Ban khong co quyen xoa du an nay."));

        var project = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (project is null)
            return Result.Fail($"GOV_INV_001: Du an khong ton tai (Id={request.Id}).");

        repository.Remove(project); // calls project.SoftDelete() → raises ProjectDeletedEvent
        await repository.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
