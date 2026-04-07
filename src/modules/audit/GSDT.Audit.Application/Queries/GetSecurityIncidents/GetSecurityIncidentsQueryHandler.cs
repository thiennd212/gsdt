using Dapper;
using FluentResults;
using MediatR;

namespace GSDT.Audit.Application.Queries.GetSecurityIncidents;

public sealed class GetSecurityIncidentsQueryHandler(IReadDbConnection db)
    : IRequestHandler<GetSecurityIncidentsQuery, Result<PagedResult<SecurityIncidentDto>>>
{
    public async Task<Result<PagedResult<SecurityIncidentDto>>> Handle(
        GetSecurityIncidentsQuery request,
        CancellationToken cancellationToken)
    {
        var p = new DynamicParameters();
        var clauses = new List<string>();

        if (request.TenantId.HasValue) { clauses.Add("TenantId = @TenantId"); p.Add("TenantId", request.TenantId); }
        if (request.Severity.HasValue) { clauses.Add("Severity = @Severity"); p.Add("Severity", (int)request.Severity); }
        if (request.Status.HasValue) { clauses.Add("Status = @Status"); p.Add("Status", request.Status.ToString()); }
        if (request.From.HasValue) { clauses.Add("OccurredAt >= @From"); p.Add("From", request.From); }
        if (request.To.HasValue) { clauses.Add("OccurredAt <= @To"); p.Add("To", request.To); }

        // Server-side search: LIKE on Title, Description, ReportedBy
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var escaped = request.SearchTerm.EscapeSqlLike();
            p.Add("SearchPattern", $"%{escaped}%");
            clauses.Add("(Title LIKE @SearchPattern ESCAPE '\\' OR Description LIKE @SearchPattern ESCAPE '\\' OR ReportedBy LIKE @SearchPattern ESCAPE '\\')");
        }

        var where = clauses.Count > 0 ? "WHERE " + string.Join(" AND ", clauses) : string.Empty;
        var offset = (request.Page - 1) * request.PageSize;
        p.Add("Offset", offset);
        p.Add("PageSize", request.PageSize);

        var total = await db.QuerySingleAsync<int>(
            $"SELECT COUNT(*) FROM audit.SecurityIncidents {where}", p, cancellationToken);

        var rows = await db.QueryAsync<SecurityIncidentDto>($"""
            SELECT Id, TenantId, Title, Severity, Status, Description, ReportedBy,
                   OccurredAt, ResolvedAt, Mitigations, CreatedAt
            FROM audit.SecurityIncidents {where}
            ORDER BY OccurredAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """, p, cancellationToken);

        var totalPages = (int)Math.Ceiling(total / (double)request.PageSize);
        var meta = new PaginationMeta(request.Page, request.PageSize, totalPages, null, null, request.Page < totalPages);
        return Result.Ok(new PagedResult<SecurityIncidentDto>(rows.ToList(), total, meta));
    }
}
