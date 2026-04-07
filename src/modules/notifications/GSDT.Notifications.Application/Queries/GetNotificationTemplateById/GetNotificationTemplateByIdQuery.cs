
namespace GSDT.Notifications.Application.Queries.GetNotificationTemplateById;

/// <summary>Returns a single notification template by ID.</summary>
public sealed record GetNotificationTemplateByIdQuery(Guid Id) : IQuery<NotificationTemplateDto>;
