
namespace GSDT.Notifications.Application.Commands.UpdateNotificationTemplate;

/// <summary>Updates SubjectTemplate and BodyTemplate of an existing notification template.</summary>
public sealed record UpdateNotificationTemplateCommand(
    Guid Id,
    string SubjectTemplate,
    string BodyTemplate) : ICommand;
