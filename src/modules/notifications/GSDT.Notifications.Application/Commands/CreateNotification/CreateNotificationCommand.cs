
namespace GSDT.Notifications.Application.Commands.CreateNotification;

/// <summary>Command to create and queue a notification for delivery.</summary>
public sealed record CreateNotificationCommand(
    Guid TenantId,
    Guid RecipientUserId,
    string Subject,
    string Body,
    string Channel) : ICommand<Guid>;
