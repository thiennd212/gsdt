using Dapper;
using GSDT.InvestmentProjects.Application.Services;
using GSDT.SharedKernel.Application.Data;
using GSDT.SharedKernel.Application.Pagination;

namespace GSDT.InvestmentProjects.Application.Queries;

/// <summary>
/// Returns a paginated list of FDI projects for the current tenant.
/// Uses Dapper + raw SQL. Same filters as NĐT/DNNN.
/// </summary>
public sealed record ListFdiProjectsQuery(
    int Page = 1,
    int PageSize = 50,
    string? Search = null,
    Guid? CompetentAuthorityId = null,
    string? InvestorName = null,
    Guid? StatusId = null,
    Guid? LocationProvinceId = null)
    : IRequest<Result<PagedResult<FdiProjectListItemDto>>>;

public sealed class ListFdiProjectsQueryHandler(
    IReadDbConnection db,
    ITenantContext tenantContext,
    IProjectQueryScopeService scopeService)
    : IRequestHandler<ListFdiProjectsQuery, Result<PagedResult<FdiProjectListItemDto>>>
{
    public async Task<Result<PagedResult<FdiProjectListItemDto>>> Handle(
        ListFdiProjectsQuery request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Result.Fail<PagedResult<FdiProjectListItemDto>>(
                "GOV_INV_401: Tenant context is required.");

        var offset = (request.Page - 1) * request.PageSize;

        var (scopeClause, scopeParams) = scopeService.GetScopeFilter("p");

        var countSql = $"""
            SELECT COUNT(*)
            FROM investment.InvestmentProjects p
            INNER JOIN investment.FdiProjects fp ON fp.Id = p.Id
            WHERE p.TenantId = @TenantId
              AND p.IsDeleted = 0
              AND (@Search IS NULL
                   OR p.ProjectCode LIKE @SearchLike
                   OR p.ProjectName LIKE @SearchLike)
              AND (@CompetentAuthorityId IS NULL OR fp.CompetentAuthorityId = @CompetentAuthorityId)
              AND (@InvestorName IS NULL OR fp.InvestorName LIKE @InvestorNameLike)
              AND (@StatusId IS NULL OR fp.StatusId = @StatusId)
              AND (@LocationProvinceId IS NULL OR EXISTS (
                  SELECT 1 FROM investment.ProjectLocations pl
                  WHERE pl.ProjectId = p.Id AND pl.ProvinceId = @LocationProvinceId))
              {scopeClause}
            """;

        var dataSql = $"""
            SELECT p.Id,
                   p.ProjectCode,
                   p.ProjectName,
                   fp.InvestorName,
                   ''              AS CompetentAuthorityName,
                   fp.PrelimTotalInvestment,
                   ''              AS StatusName,
                   p.CreatedAt
            FROM investment.InvestmentProjects p
            INNER JOIN investment.FdiProjects fp ON fp.Id = p.Id
            WHERE p.TenantId = @TenantId
              AND p.IsDeleted = 0
              AND (@Search IS NULL
                   OR p.ProjectCode LIKE @SearchLike
                   OR p.ProjectName LIKE @SearchLike)
              AND (@CompetentAuthorityId IS NULL OR fp.CompetentAuthorityId = @CompetentAuthorityId)
              AND (@InvestorName IS NULL OR fp.InvestorName LIKE @InvestorNameLike)
              AND (@StatusId IS NULL OR fp.StatusId = @StatusId)
              AND (@LocationProvinceId IS NULL OR EXISTS (
                  SELECT 1 FROM investment.ProjectLocations pl
                  WHERE pl.ProjectId = p.Id AND pl.ProvinceId = @LocationProvinceId))
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
        param.Add("CompetentAuthorityId", request.CompetentAuthorityId);
        param.Add("InvestorName", request.InvestorName);
        param.Add("InvestorNameLike", string.IsNullOrWhiteSpace(request.InvestorName)
            ? (string?)null
            : $"%{request.InvestorName.Trim()}%");
        param.Add("StatusId", request.StatusId);
        param.Add("LocationProvinceId", request.LocationProvinceId);
        param.Add("Offset", offset);
        param.Add("PageSize", request.PageSize);
        param.AddDynamicParams(scopeParams);

        var total = await db.QueryFirstOrDefaultAsync<int>(countSql, param, cancellationToken);
        var rows = await db.QueryAsync<FdiProjectListItemDto>(dataSql, param, cancellationToken);

        var totalPages = request.PageSize > 0
            ? (int)Math.Ceiling((double)total / request.PageSize)
            : 0;

        var meta = new PaginationMeta(
            request.Page, request.PageSize, totalPages,
            null, null, request.Page < totalPages);

        return Result.Ok(new PagedResult<FdiProjectListItemDto>(
            rows.ToList(), total, meta));
    }
}
