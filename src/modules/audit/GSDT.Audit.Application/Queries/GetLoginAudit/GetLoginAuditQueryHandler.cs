using Dapper;
using FluentResults;
using MediatR;

namespace GSDT.Audit.Application.Queries.GetLoginAudit;

public sealed class GetLoginAuditQueryHandler(IReadDbConnection db)
    : IRequestHandler<GetLoginAuditQuery, Result<PagedResult<LoginAttemptDto>>>
{
    public async Task<Result<PagedResult<LoginAttemptDto>>> Handle(
        GetLoginAuditQuery request,
        CancellationToken cancellationToken)
    {
        var p = new DynamicParameters();
        var clauses = new List<string>();

        if (request.UserId.HasValue) { clauses.Add("UserId = @UserId"); p.Add("UserId", request.UserId); }
        if (request.From.HasValue) { clauses.Add("AttemptedAt >= @From"); p.Add("From", request.From); }
        if (request.To.HasValue) { clauses.Add("AttemptedAt <= @To"); p.Add("To", request.To); }
        if (request.Success.HasValue) { clauses.Add("Success = @Success"); p.Add("Success", request.Success); }

        // Server-side search: LIKE on Email and IpAddress
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var escaped = request.SearchTerm.EscapeSqlLike();
            p.Add("SearchPattern", $"%{escaped}%");
            clauses.Add("(Email LIKE @SearchPattern ESCAPE '\\' OR IpAddress LIKE @SearchPattern ESCAPE '\\')");
        }

        var where = clauses.Count > 0 ? "WHERE " + string.Join(" AND ", clauses) : string.Empty;
        var offset = (request.Page - 1) * request.PageSize;
        p.Add("Offset", offset);
        p.Add("PageSize", request.PageSize);

        var total = await db.QuerySingleAsync<int>(
            $"SELECT COUNT(*) FROM audit.LoginAttempts {where}", p, cancellationToken);

        var rows = await db.QueryAsync<LoginAttemptDto>($"""
            SELECT Id, UserId, Email, IpAddress, Success, FailureReason, UserAgent, AttemptedAt
            FROM audit.LoginAttempts {where}
            ORDER BY AttemptedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """, p, cancellationToken);

        var totalPages = (int)Math.Ceiling(total / (double)request.PageSize);
        var meta = new PaginationMeta(request.Page, request.PageSize, totalPages, null, null, request.Page < totalPages);
        return Result.Ok(new PagedResult<LoginAttemptDto>(rows.ToList(), total, meta));
    }
}
