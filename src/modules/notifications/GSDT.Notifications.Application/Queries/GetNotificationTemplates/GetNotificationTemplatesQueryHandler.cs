using FluentResults;
using MediatR;

namespace GSDT.Notifications.Application.Queries.GetNotificationTemplates;

public sealed class GetNotificationTemplatesQueryHandler(IReadDbConnection db)
    : IRequestHandler<GetNotificationTemplatesQuery, Result<PagedResult<NotificationTemplateDto>>>
{
    public async Task<Result<PagedResult<NotificationTemplateDto>>> Handle(
        GetNotificationTemplatesQuery request, CancellationToken cancellationToken)
    {
        var offset = (request.Page - 1) * request.PageSize;
        var channelFilter = request.Channel is not null ? "AND Channel = @Channel" : "";

        var countSql = $"""
            SELECT COUNT(*)
            FROM notifications.NotificationTemplates
            WHERE TenantId = @TenantId AND IsDeleted = 0
            {channelFilter}
            """;

        var dataSql = $"""
            SELECT Id, TenantId, TemplateKey, SubjectTemplate, BodyTemplate,
                   Channel, IsDefault, CreatedAt, UpdatedAt
            FROM notifications.NotificationTemplates
            WHERE TenantId = @TenantId AND IsDeleted = 0
            {channelFilter}
            ORDER BY TemplateKey ASC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        var param = new { request.TenantId, request.Channel, Offset = offset, request.PageSize };

        var total = await db.QueryFirstOrDefaultAsync<int>(countSql, param, cancellationToken);
        var data = await db.QueryAsync<NotificationTemplateDto>(dataSql, param, cancellationToken);

        var totalPages = (int)Math.Ceiling(total / (double)request.PageSize);
        var meta = new PaginationMeta(request.Page, request.PageSize, totalPages, null, null, request.Page < totalPages);

        return Result.Ok(new PagedResult<NotificationTemplateDto>(data.ToList(), total, meta));
    }
}
