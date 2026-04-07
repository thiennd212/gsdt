
namespace GSDT.Notifications.Application.Commands.SendNotification;

/// <summary>
/// Unified command to send a notification via any channel.
/// Performs dedup check via NotificationLog before dispatch.
/// </summary>
public sealed record SendNotificationCommand(
    Guid TenantId,
    Guid RecipientUserId,
    string Subject,
    string Body,
    string Channel,
    Guid? TemplateId = null,
    string? CorrelationId = null) : ICommand<Guid>;
