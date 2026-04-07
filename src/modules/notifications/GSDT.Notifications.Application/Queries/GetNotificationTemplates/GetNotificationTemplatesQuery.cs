
namespace GSDT.Notifications.Application.Queries.GetNotificationTemplates;

/// <summary>Returns paginated notification templates for admin management.</summary>
public sealed record GetNotificationTemplatesQuery(
    Guid TenantId,
    int Page = 1,
    int PageSize = 20,
    string? Channel = null) : IQuery<PagedResult<NotificationTemplateDto>>;
