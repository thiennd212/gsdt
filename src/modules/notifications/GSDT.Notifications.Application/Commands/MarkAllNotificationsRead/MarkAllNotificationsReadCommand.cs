
namespace GSDT.Notifications.Application.Commands.MarkAllNotificationsRead;

/// <summary>Marks all unread notifications as read for a given user + tenant.</summary>
public sealed record MarkAllNotificationsReadCommand(Guid UserId, Guid TenantId) : ICommand;
