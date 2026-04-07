
namespace GSDT.Notifications.Application.Queries.GetNotificationById;

public sealed record GetNotificationByIdQuery(Guid Id, Guid TenantId) : IQuery<NotificationDto>;
