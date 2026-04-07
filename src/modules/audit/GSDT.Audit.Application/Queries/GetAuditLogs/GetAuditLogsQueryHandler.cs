using Dapper;
using FluentResults;
using MediatR;

namespace GSDT.Audit.Application.Queries.GetAuditLogs;

/// <summary>
/// Reads audit logs via Dapper (read-side, bypasses EF).
/// All params use DynamicParameters — no string interpolation (SQLi prevention).
/// Non-SystemAdmin requests always enforce TenantId from ambient context.
/// </summary>
public sealed class GetAuditLogsQueryHandler(
    IReadDbConnection db,
    ITenantContext tenantContext,
    IFeatureFlagService featureFlags,
    ILogger<GetAuditLogsQueryHandler> logger)
    : IRequestHandler<GetAuditLogsQuery, Result<PagedResult<AuditLogDto>>>
{
    public async Task<Result<PagedResult<AuditLogDto>>> Handle(
        GetAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        // Enforce tenant isolation: non-SystemAdmin MUST filter by ambient TenantId
        var effectiveTenantId = tenantContext.IsSystemAdmin
            ? request.TenantId  // SystemAdmin can optionally filter by any tenant
            : tenantContext.TenantId
              ?? throw new UnauthorizedAccessException("Missing tenant context for audit log access.");

        var p = new DynamicParameters();
        var where = BuildWhere(request, effectiveTenantId, p);
        var offset = (request.Page - 1) * request.PageSize;
        p.Add("Offset", offset);
        p.Add("PageSize", request.PageSize);

        var countSql = $"SELECT COUNT(*) FROM audit.AuditLogEntries {where}";
        var dataSql = $"""
            SELECT Id, TenantId, UserId, UserName, Action, ModuleName,
                   ResourceType, ResourceId, IpAddress, OccurredAt, CorrelationId
            FROM audit.AuditLogEntries
            {where}
            ORDER BY OccurredAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var total = await db.QuerySingleAsync<int>(countSql, p, cancellationToken);
        var rows = await db.QueryAsync<AuditLogDto>(dataSql, p, cancellationToken);

        var totalPages = (int)Math.Ceiling(total / (double)request.PageSize);
        var meta = new PaginationMeta(request.Page, request.PageSize, totalPages, null, null, request.Page < totalPages);

        return Result.Ok(new PagedResult<AuditLogDto>(rows.ToList(), total, meta));
    }

    /// <summary>Strip FTS special characters to prevent CONTAINS syntax errors.</summary>
    private static string SanitizeFtsInput(string input) =>
        System.Text.RegularExpressions.Regex.Replace(input, """[\"'\*\~\&\|\!\(\)\[\]\{\}\u201C\u201D]""", "").Trim();

    private string BuildWhere(GetAuditLogsQuery q, Guid? effectiveTenantId, DynamicParameters p)
    {
        var clauses = new List<string>();

        if (effectiveTenantId.HasValue) { clauses.Add("TenantId = @TenantId"); p.Add("TenantId", effectiveTenantId); }
        if (q.UserId.HasValue) { clauses.Add("UserId = @UserId"); p.Add("UserId", q.UserId); }
        if (q.From.HasValue) { clauses.Add("OccurredAt >= @From"); p.Add("From", q.From); }
        if (q.To.HasValue) { clauses.Add("OccurredAt <= @To"); p.Add("To", q.To); }
        if (!string.IsNullOrEmpty(q.Action)) { clauses.Add("Action = @Action"); p.Add("Action", q.Action); }
        if (!string.IsNullOrEmpty(q.ModuleName)) { clauses.Add("ModuleName = @ModuleName"); p.Add("ModuleName", q.ModuleName); }
        if (!string.IsNullOrEmpty(q.ResourceType)) { clauses.Add("ResourceType = @ResourceType"); p.Add("ResourceType", q.ResourceType); }
        if (!string.IsNullOrEmpty(q.ResourceId)) { clauses.Add("ResourceId = @ResourceId"); p.Add("ResourceId", q.ResourceId); }

        // Server-side search: FTS (CONTAINS) with LIKE fallback
        if (!string.IsNullOrWhiteSpace(q.SearchTerm))
        {
            var sanitized = SanitizeFtsInput(q.SearchTerm);
            if (featureFlags.IsEnabled("feature:audit.full-text-search", q.TenantId?.ToString() ?? "")
                && !string.IsNullOrWhiteSpace(sanitized))
            {
                p.Add("SearchTerm", $"\"{sanitized}\"");
                clauses.Add("CONTAINS((Action, ModuleName, ResourceType, UserName), @SearchTerm)");
            }
            else
            {
                var escaped = q.SearchTerm.EscapeSqlLike();
                p.Add("SearchPattern", $"%{escaped}%");
                clauses.Add("(Action LIKE @SearchPattern ESCAPE '\\' OR ModuleName LIKE @SearchPattern ESCAPE '\\' OR ResourceType LIKE @SearchPattern ESCAPE '\\' OR UserName LIKE @SearchPattern ESCAPE '\\')");
                logger.LogDebug("FTS disabled — using LIKE fallback for audit log search");
            }
        }

        return clauses.Count == 0 ? string.Empty : "WHERE " + string.Join(" AND ", clauses);
    }
}
