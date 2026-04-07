using Dapper;
using GSDT.InvestmentProjects.Application.Services;
using GSDT.SharedKernel.Application.Data;
using GSDT.SharedKernel.Application.Pagination;

namespace GSDT.InvestmentProjects.Application.Queries;

/// <summary>
/// Returns a paginated list of ODA investment projects for the current tenant.
/// Uses Dapper + raw SQL for performance — bypasses EF Core change tracking entirely.
/// Results are scoped by role: BTC=all, CQCQ=their authority, CDT=own projects.
/// </summary>
public sealed record ListOdaProjectsQuery(
    int Page = 1,
    int PageSize = 50,
    string? Search = null)
    : IRequest<Result<PagedResult<OdaProjectListItemDto>>>;

public sealed class ListOdaProjectsQueryHandler(
    IReadDbConnection db,
    ITenantContext tenantContext,
    IProjectQueryScopeService scopeService)
    : IRequestHandler<ListOdaProjectsQuery, Result<PagedResult<OdaProjectListItemDto>>>
{
    public async Task<Result<PagedResult<OdaProjectListItemDto>>> Handle(
        ListOdaProjectsQuery request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail<PagedResult<OdaProjectListItemDto>>(
                "GOV_INV_401: Tenant context is required.");

        var offset = (request.Page - 1) * request.PageSize;

        // Role-based scope filter (BTC="", CQCQ="AND p.ManagingAuthorityId=@...", CDT="AND p.ProjectOwnerId=@...")
        var (scopeClause, scopeParams) = scopeService.GetScopeFilter("p");

        var countSql = $"""
            SELECT COUNT(*)
            FROM investment.InvestmentProjects p
            INNER JOIN investment.OdaProjects op ON op.Id = p.Id
            WHERE p.TenantId = @TenantId
              AND p.IsDeleted = 0
              AND (@Search IS NULL
                   OR p.ProjectCode LIKE @SearchLike
                   OR p.ProjectName LIKE @SearchLike
                   OR op.ShortName   LIKE @SearchLike)
              {scopeClause}
            """;

        var dataSql = $"""
            SELECT p.Id, p.ProjectCode, p.ProjectName,
                   op.ShortName,
                   ''              AS OdaProjectTypeName,
                   p.CreatedAt,
                   ''              AS StatusName
            FROM investment.InvestmentProjects p
            INNER JOIN investment.OdaProjects op ON op.Id = p.Id
            WHERE p.TenantId = @TenantId
              AND p.IsDeleted = 0
              AND (@Search IS NULL
                   OR p.ProjectCode LIKE @SearchLike
                   OR p.ProjectName LIKE @SearchLike
                   OR op.ShortName   LIKE @SearchLike)
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
        var rows = await db.QueryAsync<OdaProjectListItemDto>(dataSql, param, cancellationToken);

        var totalPages = request.PageSize > 0
            ? (int)Math.Ceiling((double)total / request.PageSize)
            : 0;

        var meta = new PaginationMeta(
            request.Page, request.PageSize, totalPages,
            null, null, request.Page < totalPages);

        return Result.Ok(new PagedResult<OdaProjectListItemDto>(
            rows.ToList(), total, meta));
    }
}
