
namespace GSDT.Notifications.Domain.Entities;

/// <summary>
/// Notification template with Scriban rendering.
/// TemplateKey is unique per tenant + channel.
/// Templates are immutable per version — create new version instead of mutating.
/// </summary>
public sealed class NotificationTemplate : AuditableEntity<Guid>, ITenantScoped
{
    public Guid TenantId { get; private set; }

    /// <summary>Slug key — e.g. "case_submitted", "access_review_pending".</summary>
    public string TemplateKey { get; private set; } = string.Empty;

    public string SubjectTemplate { get; private set; } = string.Empty;

    /// <summary>Scriban body template. Max 20k chars. Regex timeout 100ms enforced at render.</summary>
    public string BodyTemplate { get; private set; } = string.Empty;

    public NotificationChannel Channel { get; private set; } = default!;

    public bool IsDefault { get; private set; }

    private NotificationTemplate() { } // EF Core

    public static NotificationTemplate Create(
        Guid tenantId,
        string templateKey,
        string subjectTemplate,
        string bodyTemplate,
        NotificationChannel channel,
        Guid createdBy,
        bool isDefault = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(subjectTemplate);
        ArgumentException.ThrowIfNullOrWhiteSpace(bodyTemplate);

        var template = new NotificationTemplate
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TemplateKey = templateKey.ToLowerInvariant(),
            SubjectTemplate = subjectTemplate,
            BodyTemplate = bodyTemplate,
            Channel = channel,
            IsDefault = isDefault
        };
        template.SetAuditCreate(createdBy);
        return template;
    }

    public void Update(string subjectTemplate, string bodyTemplate, Guid modifiedBy)
    {
        SubjectTemplate = subjectTemplate;
        BodyTemplate = bodyTemplate;
        SetAuditUpdate(modifiedBy);
    }

    public void ResetToDefault(string subjectTemplate, string bodyTemplate, Guid modifiedBy)
    {
        SubjectTemplate = subjectTemplate;
        BodyTemplate = bodyTemplate;
        IsDefault = true;
        SetAuditUpdate(modifiedBy);
    }
}
