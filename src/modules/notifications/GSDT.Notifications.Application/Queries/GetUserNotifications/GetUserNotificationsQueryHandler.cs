using FluentResults;
using MediatR;

namespace GSDT.Notifications.Application.Queries.GetUserNotifications;

public sealed class GetUserNotificationsQueryHandler(IReadDbConnection db)
    : IRequestHandler<GetUserNotificationsQuery, Result<PagedResult<NotificationDto>>>
{
    public async Task<Result<PagedResult<NotificationDto>>> Handle(
        GetUserNotificationsQuery request, CancellationToken cancellationToken)
    {
        var offset = (request.Page - 1) * request.PageSize;

        // Build dynamic filter conditions
        var channelFilter = request.Channel is not null ? "AND Channel = @Channel" : "";
        var readFilter = request.IsRead.HasValue ? "AND IsRead = @IsRead" : "";

        var countSql = $"""
            SELECT COUNT(*)
            FROM notifications.Notifications
            WHERE RecipientUserId = @UserId AND TenantId = @TenantId AND IsDeleted = 0
            {channelFilter} {readFilter}
            """;

        var dataSql = $"""
            SELECT Id, TenantId, RecipientUserId, Subject, Body, Channel,
                   IsRead, ReadAt, CreatedAt
            FROM notifications.Notifications
            WHERE RecipientUserId = @UserId AND TenantId = @TenantId AND IsDeleted = 0
            {channelFilter} {readFilter}
            ORDER BY CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var param = new
        {
            request.UserId,
            request.TenantId,
            request.Channel,
            request.IsRead,
            Offset = offset,
            request.PageSize
        };

        var total = await db.QueryFirstOrDefaultAsync<int>(countSql, param, cancellationToken);
        var data = await db.QueryAsync<NotificationDto>(dataSql, param, cancellationToken);

        var totalPages = (int)Math.Ceiling(total / (double)request.PageSize);
        var meta = new PaginationMeta(request.Page, request.PageSize, totalPages, null, null, request.Page < totalPages);

        return Result.Ok(new PagedResult<NotificationDto>(data.ToList(), total, meta));
    }
}
