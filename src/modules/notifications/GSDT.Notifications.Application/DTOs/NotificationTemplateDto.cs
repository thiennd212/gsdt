namespace GSDT.Notifications.Application.DTOs;

public sealed record NotificationTemplateDto(
    Guid Id,
    Guid TenantId,
    string TemplateKey,
    string SubjectTemplate,
    string BodyTemplate,
    string Channel,
    bool IsDefault,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
