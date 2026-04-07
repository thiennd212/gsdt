
namespace GSDT.Notifications.Application.Commands.CreateNotificationTemplate;

/// <summary>Creates a new notification template under the given tenant.</summary>
public sealed record CreateNotificationTemplateCommand(
    Guid TenantId,
    string TemplateKey,
    string SubjectTemplate,
    string BodyTemplate,
    string Channel) : ICommand<Guid>;
