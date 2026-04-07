
namespace GSDT.Notifications.Domain.Entities;

/// <summary>Notification aggregate root — in-app/email/sms notification record.</summary>
public sealed class Notification : AuditableEntity<Guid>, IAggregateRoot, ITenantScoped
{

    public Guid TenantId { get; private set; }
    [DataClassification(DataClassificationLevel.Internal)]
    public Guid RecipientUserId { get; private set; }
    /// <summary>Notification subject — may contain rendered PII (e.g. recipient name from template).</summary>
    [DataClassification(DataClassificationLevel.Confidential)]
    public string Subject { get; private set; } = string.Empty;
    /// <summary>Notification body — may contain rendered PII from template substitution.</summary>
    [DataClassification(DataClassificationLevel.Confidential)]
    public string Body { get; private set; } = string.Empty;
    public NotificationChannel Channel { get; private set; } = default!;
    public NotificationStatus Status { get; private set; } = NotificationStatus.Pending;
    public bool IsRead { get; private set; }
    public DateTimeOffset? ReadAt { get; private set; }
    public DateTimeOffset? SentAt { get; private set; }

    /// <summary>Optional — links notification to a template for audit trail.</summary>
    public Guid? TemplateId { get; private set; }

    /// <summary>Correlation ID from originating event (e.g. CaseId). Used for dedup.</summary>
    public string? CorrelationId { get; private set; }


    private Notification() { } // EF Core constructor

    public static Notification Create(
        Guid tenantId,
        Guid recipientUserId,
        string subject,
        string body,
        NotificationChannel channel,
        Guid createdBy,
        Guid? templateId = null,
        string? correlationId = null)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RecipientUserId = recipientUserId,
            Subject = subject,
            Body = body,
            Channel = channel,
            Status = NotificationStatus.Pending,
            TemplateId = templateId,
            CorrelationId = correlationId
        };
        notification.SetAuditCreate(createdBy);
        notification.AddDomainEvent(new NotificationCreatedEvent(
            notification.Id, tenantId, recipientUserId, channel.Value));
        return notification;
    }

    public void MarkAsSent()
    {
        Status = NotificationStatus.Sent;
        SentAt = DateTimeOffset.UtcNow;
        MarkUpdated();
    }

    public void MarkAsFailed()
    {
        Status = NotificationStatus.Failed;
        MarkUpdated();
    }

    public void MarkAsRead()
    {
        if (IsRead) return;
        IsRead = true;
        Status = NotificationStatus.Read;
        ReadAt = DateTimeOffset.UtcNow;
        MarkUpdated();
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}
