
namespace GSDT.Notifications.Application.Commands.DeleteNotificationTemplate;

/// <summary>Soft-deletes a notification template by ID.</summary>
public sealed record DeleteNotificationTemplateCommand(Guid Id) : ICommand;
