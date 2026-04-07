using Dapper;
using FluentResults;
using MediatR;

namespace GSDT.Audit.Application.Queries.GetRtbfRequests;

/// <summary>
/// List RTBF requests — NO cross-schema JOIN.
/// SubjectEmail is denormalized into RtbfRequests table (populated at creation).
/// Uses local columns only — microservice-ready.
/// </summary>
public sealed class GetRtbfRequestsQueryHandler(IReadDbConnection db)
    : IRequestHandler<GetRtbfRequestsQuery, Result<PagedResult<RtbfRequestDto>>>
{
    public async Task<Result<PagedResult<RtbfRequestDto>>> Handle(
        GetRtbfRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var p = new DynamicParameters();
        var clauses = new List<string>();

        if (request.TenantId.HasValue) { clauses.Add("TenantId = @TenantId"); p.Add("TenantId", request.TenantId); }
        if (request.Status.HasValue) { clauses.Add("Status = @Status"); p.Add("Status", (int)request.Status.Value); }

        // Search on denormalized SubjectEmail + CitizenNationalId (local columns, no cross-schema JOIN)
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var escaped = request.SearchTerm.EscapeSqlLike();
            p.Add("SearchPattern", $"%{escaped}%");
            clauses.Add("(SubjectEmail LIKE @SearchPattern ESCAPE '\\' OR CitizenNationalId LIKE @SearchPattern ESCAPE '\\')");
        }

        var where = clauses.Count > 0 ? "WHERE " + string.Join(" AND ", clauses) : string.Empty;
        var offset = (request.Page - 1) * request.PageSize;
        p.Add("Offset", offset);
        p.Add("PageSize", request.PageSize);

        var total = await db.QuerySingleAsync<int>(
            $"SELECT COUNT(*) FROM audit.RtbfRequests {where}", p, cancellationToken);

        var rows = await db.QueryAsync<RtbfRequestDto>($"""
            SELECT Id, TenantId, DataSubjectId,
                   SubjectEmail AS DataSubjectEmail,
                   CitizenNationalId,
                   CASE Status
                       WHEN 0 THEN 'Pending'
                       WHEN 1 THEN 'Processing'
                       WHEN 2 THEN 'Completed'
                       WHEN 3 THEN 'Rejected'
                       WHEN 4 THEN 'PartiallyCompleted'
                   END AS Status,
                   RequestedAt, DueBy,
                   ProcessedBy, ProcessedAt, RejectionReason
            FROM audit.RtbfRequests
            {where}
            ORDER BY RequestedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """, p, cancellationToken);

        var totalPages = (int)Math.Ceiling(total / (double)request.PageSize);
        var meta = new PaginationMeta(request.Page, request.PageSize, totalPages, null, null, request.Page < totalPages);
        return Result.Ok(new PagedResult<RtbfRequestDto>(rows.ToList(), total, meta));
    }
}
