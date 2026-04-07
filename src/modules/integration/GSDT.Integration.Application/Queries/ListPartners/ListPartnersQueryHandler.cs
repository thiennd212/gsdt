using Dapper;
using FluentResults;
using MediatR;

namespace GSDT.Integration.Application.Queries.ListPartners;

public sealed class ListPartnersQueryHandler(IReadDbConnection db)
    : IRequestHandler<ListPartnersQuery, Result<PagedResult<PartnerDto>>>
{
    public async Task<Result<PagedResult<PartnerDto>>> Handle(
        ListPartnersQuery request, CancellationToken cancellationToken)
    {
        var p = new DynamicParameters();
        p.Add("TenantId", request.TenantId);
        p.Add("Offset", (request.Page - 1) * request.PageSize);
        p.Add("PageSize", request.PageSize);

        var clauses = new List<string>
        {
            "TenantId = @TenantId",
            "IsDeleted = 0"
        };

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var escaped = request.SearchTerm.EscapeSqlLike();
            p.Add("SearchPattern", $"%{escaped}%");
            clauses.Add("(Name LIKE @SearchPattern ESCAPE '\\' OR Code LIKE @SearchPattern ESCAPE '\\' OR ContactEmail LIKE @SearchPattern ESCAPE '\\')");
        }

        var where = string.Join(" AND ", clauses);

        var countSql = $"SELECT COUNT(*) FROM integration.Partners WHERE {where}";

        var dataSql = $"""
            SELECT Id, TenantId, Name, Code, ContactEmail, ContactPhone,
                   Endpoint, AuthScheme, Status, CreatedAt, UpdatedAt
            FROM integration.Partners
            WHERE {where}
            ORDER BY Name
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var total = await db.QuerySingleAsync<int>(countSql, p, cancellationToken);
        var items = (await db.QueryAsync<PartnerDto>(dataSql, p, cancellationToken)).ToList();

        var totalPages = (int)Math.Ceiling(total / (double)request.PageSize);
        var meta = new PaginationMeta(request.Page, request.PageSize, totalPages,
            null, null, request.Page < totalPages);

        return Result.Ok(new PagedResult<PartnerDto>(items, total, meta));
    }
}
