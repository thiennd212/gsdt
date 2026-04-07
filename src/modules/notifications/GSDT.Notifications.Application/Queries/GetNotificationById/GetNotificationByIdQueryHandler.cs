using FluentResults;
using MediatR;

namespace GSDT.Notifications.Application.Queries.GetNotificationById;

public sealed class GetNotificationByIdQueryHandler(IReadDbConnection db)
    : IRequestHandler<GetNotificationByIdQuery, Result<NotificationDto>>
{
    public async Task<Result<NotificationDto>> Handle(
        GetNotificationByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Query handler uses Dapper directly — bypasses IRepository (read side)
        const string sql = """
            SELECT Id, TenantId, RecipientUserId, Subject, Body, Channel,
                   IsRead, ReadAt, CreatedAt
            FROM notifications.Notifications
            WHERE Id = @Id AND TenantId = @TenantId AND IsDeleted = 0
            """;

        var dto = await db.QueryFirstOrDefaultAsync<NotificationDto>(
            sql, new { request.Id, request.TenantId }, cancellationToken);

        return dto is null
            ? Result.Fail(new NotFoundError($"Notification {request.Id} not found."))
            : Result.Ok(dto);
    }
}
