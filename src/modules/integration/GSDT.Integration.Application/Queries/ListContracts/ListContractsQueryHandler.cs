using Dapper;
using FluentResults;
using MediatR;

namespace GSDT.Integration.Application.Queries.ListContracts;

public sealed class ListContractsQueryHandler(IReadDbConnection db)
    : IRequestHandler<ListContractsQuery, Result<PagedResult<ContractDto>>>
{
    public async Task<Result<PagedResult<ContractDto>>> Handle(
        ListContractsQuery request, CancellationToken cancellationToken)
    {
        var p = new DynamicParameters();
        p.Add("TenantId", request.TenantId);
        p.Add("PartnerId", request.PartnerId);
        p.Add("Offset", (request.Page - 1) * request.PageSize);
        p.Add("PageSize", request.PageSize);

        var clauses = new List<string>
        {
            "TenantId = @TenantId",
            "IsDeleted = 0",
            "(@PartnerId IS NULL OR PartnerId = @PartnerId)"
        };

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var escaped = request.SearchTerm.EscapeSqlLike();
            p.Add("SearchPattern", $"%{escaped}%");
            clauses.Add("(Title LIKE @SearchPattern ESCAPE '\\' OR Description LIKE @SearchPattern ESCAPE '\\')");
        }

        var where = string.Join(" AND ", clauses);

        var countSql = $"SELECT COUNT(*) FROM integration.Contracts WHERE {where}";

        var dataSql = $"""
            SELECT Id, TenantId, PartnerId, Title, Description,
                   EffectiveDate, ExpiryDate, Status, DataScopeJson,
                   CreatedAt, UpdatedAt
            FROM integration.Contracts
            WHERE {where}
            ORDER BY EffectiveDate DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var total = await db.QuerySingleAsync<int>(countSql, p, cancellationToken);
        var items = (await db.QueryAsync<ContractDto>(dataSql, p, cancellationToken)).ToList();

        var totalPages = (int)Math.Ceiling(total / (double)request.PageSize);
        var meta = new PaginationMeta(request.Page, request.PageSize, totalPages,
            null, null, request.Page < totalPages);

        return Result.Ok(new PagedResult<ContractDto>(items, total, meta));
    }
}
