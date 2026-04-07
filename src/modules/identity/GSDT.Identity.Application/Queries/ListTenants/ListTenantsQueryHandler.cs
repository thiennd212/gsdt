using Dapper;
using FluentResults;
using MediatR;

namespace GSDT.Identity.Application.Queries.ListTenants;

/// <summary>Read-side query via Dapper — returns distinct tenants from AspNetUsers.</summary>
public sealed class ListTenantsQueryHandler(IReadDbConnection db)
    : IRequestHandler<ListTenantsQuery, Result<IReadOnlyList<TenantSummaryDto>>>
{
    public async Task<Result<IReadOnlyList<TenantSummaryDto>>> Handle(
        ListTenantsQuery query, CancellationToken ct)
    {
        // Join root OrgUnit (ParentId IS NULL) to get tenant display name
        const string sql = """
            SELECT u.TenantId, o.Name AS TenantName, COUNT(*) AS UserCount
            FROM [identity].AspNetUsers u
            LEFT JOIN [organization].OrgUnits o ON o.TenantId = u.TenantId AND o.ParentId IS NULL
            WHERE u.TenantId IS NOT NULL
            GROUP BY u.TenantId, o.Name
            ORDER BY UserCount DESC
            """;

        var rows = await db.QueryAsync<TenantSummaryDto>(sql, new { }, ct);
        return Result.Ok<IReadOnlyList<TenantSummaryDto>>(rows.ToList());
    }
}
