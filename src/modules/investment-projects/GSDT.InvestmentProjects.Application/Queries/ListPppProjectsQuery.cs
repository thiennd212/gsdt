using Dapper;
using GSDT.InvestmentProjects.Application.Services;
using GSDT.SharedKernel.Application.Data;
using GSDT.SharedKernel.Application.Pagination;

namespace GSDT.InvestmentProjects.Application.Queries;

/// <summary>
/// Returns a paginated list of PPP projects for the current tenant.
/// Uses Dapper + raw SQL for performance — bypasses EF Core change tracking.
/// Filterable by search text, contract type, competent authority, and status.
/// Results are scoped by role: BTC=all, CQCQ=their authority, CDT=own projects.
/// </summary>
public sealed record ListPppProjectsQuery(
    int Page = 1,
    int PageSize = 50,
    string? Search = null,
    int? ContractType = null,
    Guid? CompetentAuthorityId = null,
    Guid? StatusId = null)
    : IRequest<Result<PagedResult<PppProjectListItemDto>>>;

public sealed class ListPppProjectsQueryHandler(
    IReadDbConnection db,
    ITenantContext tenantContext,
    IProjectQueryScopeService scopeService)
    : IRequestHandler<ListPppProjectsQuery, Result<PagedResult<PppProjectListItemDto>>>
{
    public async Task<Result<PagedResult<PppProjectListItemDto>>> Handle(
        ListPppProjectsQuery request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail<PagedResult<PppProjectListItemDto>>(
                "GOV_INV_401: Tenant context is required.");

        var offset = (request.Page - 1) * request.PageSize;

        // Role-based scope filter (BTC="", CQCQ="AND p.ManagingAuthorityId=@...", CDT="AND p.ProjectOwnerId=@...")
        var (scopeClause, scopeParams) = scopeService.GetScopeFilter("p");

        var countSql = $"""
            SELECT COUNT(*)
            FROM investment.InvestmentProjects p
            INNER JOIN investment.PppProjects pp ON pp.Id = p.Id
            WHERE p.TenantId = @TenantId
              AND p.IsDeleted = 0
              AND (@Search IS NULL
                   OR p.ProjectCode LIKE @SearchLike
                   OR p.ProjectName LIKE @SearchLike)
              AND (@ContractType IS NULL OR pp.ContractType = @ContractType)
              AND (@CompetentAuthorityId IS NULL OR pp.CompetentAuthorityId = @CompetentAuthorityId)
              AND (@StatusId IS NULL OR pp.StatusId = @StatusId)
              {scopeClause}
            """;

        var dataSql = $"""
            SELECT p.Id,
                   p.ProjectCode,
                   p.ProjectName,
                   pp.ContractType,
                   ''              AS CompetentAuthorityName,
                   pp.PreparationUnit,
                   pp.PrelimTotalInvestment,
                   ''              AS StatusName,
                   p.CreatedAt
            FROM investment.InvestmentProjects p
            INNER JOIN investment.PppProjects pp ON pp.Id = p.Id
            WHERE p.TenantId = @TenantId
              AND p.IsDeleted = 0
              AND (@Search IS NULL
                   OR p.ProjectCode LIKE @SearchLike
                   OR p.ProjectName LIKE @SearchLike)
              AND (@ContractType IS NULL OR pp.ContractType = @ContractType)
              AND (@CompetentAuthorityId IS NULL OR pp.CompetentAuthorityId = @CompetentAuthorityId)
              AND (@StatusId IS NULL OR pp.StatusId = @StatusId)
              {scopeClause}
            ORDER BY p.CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var param = new DynamicParameters();
        param.Add("TenantId", tenantId);
        param.Add("Search", request.Search);
        param.Add("SearchLike", string.IsNullOrWhiteSpace(request.Search)
            ? (string?)null
            : $"%{request.Search.Trim()}%");
        param.Add("ContractType", request.ContractType);
        param.Add("CompetentAuthorityId", request.CompetentAuthorityId);
        param.Add("StatusId", request.StatusId);
        param.Add("Offset", offset);
        param.Add("PageSize", request.PageSize);
        param.AddDynamicParams(scopeParams);

        var total = await db.QueryFirstOrDefaultAsync<int>(countSql, param, cancellationToken);
        var rows = await db.QueryAsync<PppProjectListItemDto>(dataSql, param, cancellationToken);

        var totalPages = request.PageSize > 0
            ? (int)Math.Ceiling((double)total / request.PageSize)
            : 0;

        var meta = new PaginationMeta(
            request.Page, request.PageSize, totalPages,
            null, null, request.Page < totalPages);

        return Result.Ok(new PagedResult<PppProjectListItemDto>(
            rows.ToList(), total, meta));
    }
}
