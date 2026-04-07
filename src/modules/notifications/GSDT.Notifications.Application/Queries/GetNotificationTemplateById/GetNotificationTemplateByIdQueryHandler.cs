using FluentResults;
using MediatR;

namespace GSDT.Notifications.Application.Queries.GetNotificationTemplateById;

public sealed class GetNotificationTemplateByIdQueryHandler(IReadDbConnection db)
    : IRequestHandler<GetNotificationTemplateByIdQuery, Result<NotificationTemplateDto>>
{
    public async Task<Result<NotificationTemplateDto>> Handle(
        GetNotificationTemplateByIdQuery request,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT Id, TenantId, TemplateKey, SubjectTemplate, BodyTemplate,
                   Channel, IsDefault, CreatedAt, UpdatedAt
            FROM notifications.NotificationTemplates
            WHERE Id = @Id AND IsDeleted = 0
            """;

        var dto = await db.QueryFirstOrDefaultAsync<NotificationTemplateDto>(
            sql, new { request.Id }, cancellationToken);

        if (dto is null)
            return Result.Fail(new NotFoundError($"Notification template {request.Id} not found."));

        return Result.Ok(dto);
    }
}
