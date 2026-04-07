
namespace GSDT.Notifications.Application.Commands.MarkNotificationRead;

/// <summary>Marks a single notification as read. Only the owning user may mark their own.</summary>
public sealed record MarkNotificationReadCommand(Guid NotificationId, Guid TenantId, Guid UserId) : ICommand;
