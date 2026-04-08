using Dapper;
using GSDT.InvestmentProjects.Application.Services;
using GSDT.SharedKernel.Application.Data;
using GSDT.SharedKernel.Application.Pagination;

namespace GSDT.InvestmentProjects.Application.Queries;

/// <summary>
/// Returns a paginated list of domestic investment projects for the current tenant.
/// Uses Dapper + raw SQL for performance — bypasses EF Core change tracking entirely.
/// Joins latest decision via ROW_NUMBER window function.
/// Results are scoped by role: BTC=all, CQCQ=their authority, CDT=own projects.
/// </summary>
public sealed record ListDomesticProjectsQuery(
    int Page = 1,
    int PageSize = 50,
    string? Search = null)
    : IRequest<Result<PagedResult<DomesticProjectListItemDto>>>;

public sealed class ListDomesticProjectsQueryHandler(
    IReadDbConnection db,
    ITenantContext tenantContext,
    IProjectQueryScopeService scopeService)
    : IRequestHandler<ListDomesticProjectsQuery, Result<PagedResult<DomesticProjectListItemDto>>>
{
    public async Task<Result<PagedResult<DomesticProjectListItemDto>>> Handle(
        ListDomesticProjectsQuery request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail<PagedResult<DomesticProjectListItemDto>>(
                "GOV_INV_401: Tenant context is required.");

        var offset = (request.Page - 1) * request.PageSize;

        // Role-based scope filter (BTC="", CQCQ="AND p.ManagingAuthorityId=@...", CDT="AND p.ProjectOwnerId=@...")
        var (scopeClause, scopeParams) = scopeService.GetScopeFilter("p");

        var countSql = $"""
            SELECT COUNT(*)
            FROM investment.InvestmentProjects p
            INNER JOIN investment.DomesticProjects dp ON dp.Id = p.Id
            WHERE p.TenantId = @TenantId
              AND p.IsDeleted = 0
              AND (@Search IS NULL
                   OR p.ProjectCode LIKE @SearchLike
                   OR p.ProjectName LIKE @SearchLike)
              {scopeClause}
            """;

        var dataSql = $"""
            SELECT p.Id, p.ProjectCode, p.ProjectName,
                   d.DecisionNumber AS LatestDecisionNumber,
                   d.DecisionDate   AS LatestDecisionDate,
                   ''               AS ProjectOwnerName,
                   ''               AS StatusName
            FROM investment.InvestmentProjects p
            INNER JOIN investment.DomesticProjects dp ON dp.Id = p.Id
            LEFT JOIN (
                SELECT ProjectId, DecisionNumber, DecisionDate,
                       ROW_NUMBER() OVER (PARTITION BY ProjectId ORDER BY DecisionDate DESC) AS rn
                FROM investment.DomesticInvestmentDecisions
                WHERE IsDeleted = 0
            ) d ON d.ProjectId = p.Id AND d.rn = 1
            WHERE p.TenantId = @TenantId
              AND p.IsDeleted = 0
              AND (@Search IS NULL
                   OR p.ProjectCode LIKE @SearchLike
                   OR p.ProjectName LIKE @SearchLike)
              {scopeClause}
            ORDER BY p.CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        // Merge base params with scope params using DynamicParameters
        var param = new DynamicParameters();
        param.Add("TenantId", tenantId);
        param.Add("Search", request.Search);
        param.Add("SearchLike", string.IsNullOrWhiteSpace(request.Search)
            ? (string?)null
            : $"%{request.Search.Trim()}%");
        param.Add("Offset", offset);
        param.Add("PageSize", request.PageSize);
        param.AddDynamicParams(scopeParams);

        var total = await db.QueryFirstOrDefaultAsync<int>(countSql, param, cancellationToken);
        var rows = await db.QueryAsync<DomesticProjectListItemDto>(dataSql, param, cancellationToken);

        var totalPages = request.PageSize > 0
            ? (int)Math.Ceiling((double)total / request.PageSize)
            : 0;

        var meta = new PaginationMeta(
            request.Page, request.PageSize, totalPages,
            null, null, request.Page < totalPages);

        return Result.Ok(new PagedResult<DomesticProjectListItemDto>(
            rows.ToList(), total, meta));
    }
}
