using FluentResults;
using MediatR;

namespace GSDT.Integration.Application.Queries.GetMessageLog;

public sealed class GetMessageLogQueryHandler(IReadDbConnection db)
    : IRequestHandler<GetMessageLogQuery, Result<MessageLogDto>>
{
    public async Task<Result<MessageLogDto>> Handle(
        GetMessageLogQuery request, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT Id, TenantId, PartnerId, ContractId,
                   Direction, MessageType, Payload, Status,
                   CorrelationId, SentAt, AcknowledgedAt,
                   CreatedAt, UpdatedAt
            FROM integration.MessageLogs
            WHERE Id = @Id AND TenantId = @TenantId AND IsDeleted = 0
            """;

        var row = await db.QueryFirstOrDefaultAsync<MessageLogDto>(
            sql, new { request.Id, request.TenantId }, cancellationToken);

        return row is null
            ? Result.Fail(new NotFoundError($"MessageLog {request.Id} not found."))
            : Result.Ok(row);
    }
}
