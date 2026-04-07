
namespace GSDT.Notifications.Application.Queries.GetUnreadNotificationCount;

/// <summary>Returns the unread in-app notification count for the current user.</summary>
public sealed record GetUnreadNotificationCountQuery(Guid UserId, Guid TenantId) : IQuery<int>;
