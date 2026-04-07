namespace GSDT.InvestmentProjects.Application.Services;

/// <summary>
/// Provides role-based query scoping and write authorization for investment projects.
/// BTC (Bo Tai chinh) = system-wide read/write.
/// CQCQ (Co quan chu quan) = read-only, scoped to their managing authority.
/// CDT (Chu dau tu) = read/write their own projects only.
/// </summary>
public interface IProjectQueryScopeService
{
    /// <summary>
    /// Returns a SQL WHERE fragment and parameters for scoping list queries.
    /// BTC returns empty string (no additional filter).
    /// CQCQ filters by ManagingAuthorityId from the current user's JWT claims.
    /// CDT filters by ProjectOwnerId from the current user's JWT claims.
    /// </summary>
    (string WhereClause, object Parameters) GetScopeFilter(string projectAlias = "p");

    /// <summary>
    /// Returns true if the current user is allowed to create/update/delete the given project.
    /// BTC: always true. CQCQ: always false (read-only). CDT: true only when project.ProjectOwnerId matches.
    /// </summary>
    Task<bool> CanModifyProjectAsync(Guid projectId, CancellationToken ct = default);
}
