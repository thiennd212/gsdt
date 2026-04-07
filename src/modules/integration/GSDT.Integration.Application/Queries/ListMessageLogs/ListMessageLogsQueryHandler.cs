using Dapper;
using FluentResults;
using MediatR;

namespace GSDT.Integration.Application.Queries.ListMessageLogs;

public sealed class ListMessageLogsQueryHandler(IReadDbConnection db)
    : IRequestHandler<ListMessageLogsQuery, Result<PagedResult<MessageLogDto>>>
{
    public async Task<Result<PagedResult<MessageLogDto>>> Handle(
        ListMessageLogsQuery request, CancellationToken cancellationToken)
    {
        var p = new DynamicParameters();
        p.Add("TenantId", request.TenantId);
        p.Add("PartnerId", request.PartnerId);
        p.Add("ContractId", request.ContractId);
        p.Add("Offset", (request.Page - 1) * request.PageSize);
        p.Add("PageSize", request.PageSize);

        var clauses = new List<string>
        {
            "TenantId = @TenantId",
            "IsDeleted = 0",
            "(@PartnerId IS NULL OR PartnerId = @PartnerId)",
            "(@ContractId IS NULL OR ContractId = @ContractId)"
        };

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var escaped = request.SearchTerm.EscapeSqlLike();
            p.Add("SearchPattern", $"%{escaped}%");
            clauses.Add("(Direction LIKE @SearchPattern ESCAPE '\\' OR MessageType LIKE @SearchPattern ESCAPE '\\')");
        }

        var where = string.Join(" AND ", clauses);

        var countSql = $"SELECT COUNT(*) FROM integration.MessageLogs WHERE {where}";

        var dataSql = $"""
            SELECT Id, TenantId, PartnerId, ContractId,
                   Direction, MessageType, Payload, Status,
                   CorrelationId, SentAt, AcknowledgedAt,
                   CreatedAt, UpdatedAt
            FROM integration.MessageLogs
            WHERE {where}
            ORDER BY SentAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var total = await db.QuerySingleAsync<int>(countSql, p, cancellationToken);
        var items = (await db.QueryAsync<MessageLogDto>(dataSql, p, cancellationToken)).ToList();

        var totalPages = (int)Math.Ceiling(total / (double)request.PageSize);
        var meta = new PaginationMeta(request.Page, request.PageSize, totalPages,
            null, null, request.Page < totalPages);

        return Result.Ok(new PagedResult<MessageLogDto>(items, total, meta));
    }
}
