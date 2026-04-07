using Dapper;
using FluentResults;
using MediatR;

namespace GSDT.Audit.Application.Queries.GetAiPromptTraces;

/// <summary>
/// Reads AI prompt traces via Dapper (read-side).
/// PromptText is excluded from SELECT — sensitive field never exposed via query.
/// </summary>
public sealed class GetAiPromptTracesQueryHandler(IReadDbConnection db)
    : IRequestHandler<GetAiPromptTracesQuery, Result<PagedResult<AiPromptTraceDto>>>
{
    public async Task<Result<PagedResult<AiPromptTraceDto>>> Handle(
        GetAiPromptTracesQuery request,
        CancellationToken cancellationToken)
    {
        var p = new DynamicParameters();
        p.Add("TenantId", request.TenantId);
        p.Add("Offset", (request.Page - 1) * request.PageSize);
        p.Add("PageSize", request.PageSize);

        var clauses = new List<string> { "TenantId = @TenantId" };

        // Server-side search: LIKE on ModelName and ClassificationLevel
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var escaped = request.SearchTerm.EscapeSqlLike();
            p.Add("SearchPattern", $"%{escaped}%");
            clauses.Add("(ModelName LIKE @SearchPattern ESCAPE '\\' OR ClassificationLevel LIKE @SearchPattern ESCAPE '\\')");
        }

        var where = "WHERE " + string.Join(" AND ", clauses);

        var countSql = $"SELECT COUNT(*) FROM audit.AiPromptTraces {where}";
        var dataSql = $"""
            SELECT Id, SessionId, ModelProfileId, ModelName, PromptHash,
                   ResponseHash, InputTokens, OutputTokens, LatencyMs,
                   Cost, ClassificationLevel, TenantId, CreatedAt
            FROM audit.AiPromptTraces
            {where}
            ORDER BY CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var total = await db.QuerySingleAsync<int>(countSql, p, cancellationToken);
        var rows = await db.QueryAsync<AiPromptTraceDto>(dataSql, p, cancellationToken);

        var totalPages = (int)Math.Ceiling(total / (double)request.PageSize);
        var meta = new PaginationMeta(request.Page, request.PageSize, totalPages, null, null, request.Page < totalPages);

        return Result.Ok(new PagedResult<AiPromptTraceDto>(rows.ToList(), total, meta));
    }
}
