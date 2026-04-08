using GSDT.InvestmentProjects.Application.Services;
using GSDT.InvestmentProjects.Infrastructure.Persistence;

namespace GSDT.InvestmentProjects.Infrastructure.Services;

/// <summary>
/// Role-based query scoping and write authorization for investment projects.
/// Reads role and scope claims from ICurrentUser (JWT-backed in production, stub in tests).
/// </summary>
public sealed class ProjectQueryScopeService(
    ICurrentUser currentUser,
    InvestmentProjectsDbContext db) : IProjectQueryScopeService
{
    public (string WhereClause, object Parameters) GetScopeFilter(string projectAlias = "p")
    {
        // BTC = system-wide access, no additional filter
        if (currentUser.Roles.Contains("BTC"))
            return ("", new { });

        // CQCQ = read-only, scoped to managing authority
        if (currentUser.Roles.Contains("CQCQ"))
        {
            var authorityId = currentUser.ManagingAuthorityId
                ?? throw new UnauthorizedAccessException(
                    "Nguoi dung CQCQ chua co thong tin co quan chu quan (managing_authority_id).");
            return (
                $"AND {projectAlias}.ManagingAuthorityId = @ScopeAuthorityId",
                new { ScopeAuthorityId = authorityId });
        }

        // CDT = own projects only, scoped to project owner
        if (currentUser.Roles.Contains("CDT"))
        {
            var ownerId = currentUser.ProjectOwnerId
                ?? throw new UnauthorizedAccessException(
                    "Nguoi dung CDT chua co thong tin chu dau tu (project_owner_id).");
            return (
                $"AND {projectAlias}.ProjectOwnerId = @ScopeOwnerId",
                new { ScopeOwnerId = ownerId });
        }

        throw new UnauthorizedAccessException(
            "Nguoi dung khong co quyen truy cap du lieu du an dau tu.");
    }

    public async Task<bool> CanModifyProjectAsync(Guid projectId, CancellationToken ct = default)
    {
        // BTC has full system-wide write access
        if (currentUser.Roles.Contains("BTC"))
            return true;

        // CQCQ is read-only — cannot modify any project
        if (currentUser.Roles.Contains("CQCQ"))
            return false;

        // CDT can only modify projects they own
        if (currentUser.Roles.Contains("CDT"))
        {
            var ownerId = currentUser.ProjectOwnerId;
            if (ownerId is null) return false;

            var project = await db.InvestmentProjects
                .Where(p => p.Id == projectId && !p.IsDeleted)
                .Select(p => new { p.ProjectOwnerId })
                .FirstOrDefaultAsync(ct);

            return project?.ProjectOwnerId == ownerId;
        }

        return false;
    }
}
