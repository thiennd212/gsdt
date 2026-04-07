
namespace GSDT.Notifications.Application.Queries.GetUserNotifications;

/// <summary>Returns paginated notifications for the current user. Filterable by channel and read status.</summary>
public sealed record GetUserNotificationsQuery(
    Guid UserId,
    Guid TenantId,
    int Page = 1,
    int PageSize = 20,
    string? Channel = null,
    bool? IsRead = null) : IQuery<PagedResult<NotificationDto>>;
